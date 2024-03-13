using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Db.PodcastMgt.PowerTools.Models;
using PodCatcher.Helpers;

//namespace PodCatcher.Helpers
namespace PodCatcher.ViewModels
{
  partial class PodCatcherViewModel
  {
    public bool IsAutoAll = false;
    DataGrid _dgD;

    public bool LaunchIfDownloadRequiredMvvm(PodcastMgtContext db, DnLd dl, DataGrid dg)
    {
      Debug.WriteLine("?:>{0}  \t{1}", dl.Feed.Name, dl.CastTitle);

      if (!IsDownloadRequiredWrpr(dl)) return false;

      StartDownload(dl, dg);
      return true;
    }
    public static bool IsDownloadRequiredWrpr(DnLd dl)
    {
      var reason = "";
      var isRequired = PodHelper.IsDownloadRequired(dl, ref reason);
      dl.RunTimeNote = $"<rsn={reason}> <{dl.CastFilenameExt}>";
      return isRequired;
    }
    public void StartDownload(DnLd dnLd, DataGrid dgD)
    {
      _dgD = dgD;

      startDownload(dnLd);

      lock (thisLock) { var i = TtlDnLds2Finish; Interlocked.Increment(ref i); TtlDnLds2Finish = i; }
    }

    void startDownload(DnLd dnld)
    {
      var dfe = dnld.FullPathFile(MiscHelper.DirPlyr);
      var dir = Path.GetDirectoryName(dfe);
      if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

      var wc = new WebClient();
      wc.Proxy = WebRequest.DefaultWebProxy; //TU: 1/2
      wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials; //TU: 2/2
      wc.BaseAddress = dnld.CastUrl;
      wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
      wc.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(wc_DownloadFileCompleted);
      wc.DownloadFileAsync(new Uri(dnld.CastUrl), dfe);

      dnld.DnldStatusId = "I"; // Is Being Downloaded
      dnld.DownloadStart = DateTime.Now;
      dnld.DownloadedAt = null;
      //no need here as there is no changes yet: if (dgD != null) Application.Current.Dispatcher.BeginInvoke(new Action(() => dgD.Items.Refresh()));
    }
    DateTime _nextRefreshTime = DateTime.MinValue;
    void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      var now = DateTime.Now;
      if (now < _nextRefreshTime) return;

      _nextRefreshTime = now.AddSeconds(1);

      Debug.WriteLine("{0:HH:mm:ss.fff} {1,11} / {2,11}  {3,5}%  {4}", DateTime.Now, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage, ((WebClient)(sender)).BaseAddress);

      var dr = getDnldRowFormDB(db, ((WebClient)(sender)).BaseAddress);

      dr.DownloadedLength = e.BytesReceived;

      //if (e.BytesReceived == -1)				updateDb_doPostDnld(dr);

      if (_dgD != null && Application.Current != null)
        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
          try { _dgD.Items.Refresh(); } //todo: better check if is editing
          catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
        }));      //_dg.Items.Refresh();
    }
    async void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      Bpr.BeepOk();
      lock (thisLock) { var i = TtlDnLds2Finish; Interlocked.Decrement(ref i); TtlDnLds2Finish = i; }

      var dr = getDnldRowFormDB(db, ((WebClient)(sender)).BaseAddress);

      if (dr.CastFileLength < dr.DownloadedLength) dr.CastFileLength = dr.DownloadedLength;

      if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
      {
        InfoApnd =
            $"Oops! Download of {dr.CastTitle} has failed with the error {e.Error.InnermostMessage()}. {TtlDnLds2Finish} downloads to go.";
        dr.DnldStatusId = "F"; // Failed to Download
        dr.ErrLog += $"\nDnld failed: {e.Error}";
        if (dr.ErrLog.Length > 1020) dr.ErrLog = dr.ErrLog.Substring(0, 1020);  // Trunkate(dr.ErrLog, 1000);
        onSaveChanges(null);
        //reStart(); <- fails immediately => redo after the second round of FeedCheck and such.
      }
      else
      {
        await updateDb_doPostDnld(dr);

        if (TtlDnLds2Finish > 0)
          InfoApnd = $"download of {dr.CastTitle} has succeeded. {TtlDnLds2Finish} more to go.";
        else
        {
          InfoApnd = string.Format("All downloads have finished. Pausing for a bit to get cutting of the last download to start.");
          await Task.Delay(10333); //let the last downloaded file to start creating cut folder (lest launch two cutters for the same file).
          InfoApnd = string.Format("Pausing is over: movin on to final anons generation.");
          await onGenerateAnonces(null);
          InfoApnd = string.Format("Final anons generation is complete.");

          if (IsAutoNextStep)
          {
            InfoApnd = string.Format("Re-Starting feeds check now.");
            checkAllFeeds(_pnl1);
          }
        }

      }
    }

    public static void Truncate(ref string s, int max)
    {
      if (s.Length >= max - 1)
        s = s.Substring(0, max - 2);
    }

    async Task updateDb_doPostDnld(DnLd dr)
    {
      dr.DownloadedAt = DateTime.Now;
      dr.DownloadedByPC = Environment.MachineName;
      dr.DownloadedToDir = MiscHelper.DirPlyr;
      dr.DnldStatusId = "H"; // HasBeenDownloaded
      dr.ReDownload = false;
      onSaveChanges(null);

      if (Environment.MachineName == "LN1") return;

      //Task.Factory.StartNew(() => PostDnldHelper.DoPostDownloadProcessing(dr)).ContinueWith(_ => 				{
      await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
              await PostDnldHelper.DoPostDownloadProcessing(dr);

              dr.DnldStatusId = "A"; // All done
              dr.ReDownload = false;
              if (_dgD != null) _dgD.Items.Refresh();
              //}, TaskScheduler.FromCurrentSynchronizationContext());
            }));
    }

    static DnLd getDnldRowFormDB(PodcastMgtContext db, string url)
    {
      lock (thisLock)
      {
        do
        {
          try
          {
            var dr = db.DnLds.FirstOrDefault(r => r.CastUrl == url);
            if (dr == null)
              dr = db.DnLds.FirstOrDefault(r => r.CastUrl.Contains(Path.GetFileName(url)));
            if (dr == null)
              throw new Exception("Oops. Another problem to fix.");
            return dr;
          }
          catch (Exception ex) { ex.Log(); }
        } while (true);
      }
    }
  }
}
