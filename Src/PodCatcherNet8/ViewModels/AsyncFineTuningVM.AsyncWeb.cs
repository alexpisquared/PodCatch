using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Db.PodcastMgt.PowerTools.Models;

namespace PodCatcher.ViewModels
{
  public partial class AsyncFineTuningVM : BindableBaseViewModel
  {
    async Task asyUpdtFeedsCT(CancellationToken ct, List<Feed> feedList)
    {
      using (var handler = new HttpClientHandler())// { Credentials = ... })
      {
        handler.UseDefaultCredentials = true;
        handler.Proxy = WebRequest.DefaultWebProxy;
        handler.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

        using (var client = new HttpClient(handler))
        {
          var dnldTasksQuery = from feed in feedList select updateFeedFromWebTaskPP(feed, client, ct); // ***Create a query that, when executed, returns a collection of tasks.
          var taskList = dnldTasksQuery.ToList();                                                              // ***Use ToList to execute the query and start the tasks. 

          Max1 = taskList.Count;
          while (taskList.Count > 0)                                                                                        // ***Add a loop to process the tasks one at a time until none remain.
          {
            var firstFinishedTask = await Task.WhenAny(taskList);                                                    // Identify the first task that completes.
            taskList.Remove(firstFinishedTask);                                                                             // ***Remove the selected task from the list so that you don't process it more than once.
            var feedCopyOrWhat = await firstFinishedTask;     // InfoMsg += string.Format("\r\n RV:  {0,4}  '{1}' {2}", feedCopyOrWhat.Id, feedCopyOrWhat.ErrLog, "");
            Val1++;
          }
        }
      }
    }
    async Task asyUpdtDnLdsCT(CancellationToken ct)
    {
      using (var handler = new HttpClientHandler())// { Credentials = ... })
      {
        handler.UseDefaultCredentials = true;
        handler.Proxy = WebRequest.DefaultWebProxy;
        handler.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

        using (var client = new HttpClient(handler))
        {
          var dnldTasksQuery = from dnLd in _db.DnLds.Where(r => r.ReDownload).ToList() select updateDnLdFromWebTaskPP(dnLd, client, ct); // ***Create a query that, when executed, returns a collection of tasks.
          var taskList = dnldTasksQuery.ToList();                                                              // ***Use ToList to execute the query and start the tasks. 

          Max3 = taskList.Count;
          while (taskList.Count > 0)                                                                                        // ***Add a loop to process the tasks one at a time until none remain.
          {
            var firstFinishedTask = await Task.WhenAny(taskList);                                                    // Identify the first task that completes.
            taskList.Remove(firstFinishedTask);                                                                             // ***Remove the selected task from the list so that you don't process it more than once.

            var dnLdCopyOrWhat = await firstFinishedTask;     // InfoMsg += string.Format("\r\n RV:  {0,4}  '{1}' {2}", dnLdCopyOrWhat.Id, dnLdCopyOrWhat.ErrLog, "");
            dnLdCopyOrWhat.RunTimeNote = string.Format("Dnloaded, eh?");
            if (dnLdCopyOrWhat.DnldStatusId == "I") //dbl chk/assign-t to H.
            {
              dnLdCopyOrWhat.DnldStatusId = "H";
              dnLdCopyOrWhat.ReDownload = false;
            }
            Val3++;
          }
        }
      }
    }


    async Task<Feed> updateFeedFromWebTaskPP(Feed feed, HttpClient client, CancellationToken ct) // https://msdn.microsoft.com/en-ca/library/jj155756.aspx
    {
      Debug.WriteLine("Going for feed ID {0,4}  URL {1}", feed.Id, feed.Url);

      feed.RunTimeNote = string.Format("F1 ... ");

      if (!(string.IsNullOrEmpty(feed.Url) || feed.Url == "local"))
        try
        {
          var response = await client.GetAsync(feed.Url, ct);

          var latestRssText = await response.Content.ReadAsStringAsync(); //byte[]  RssText = await response.Content.ReadAsByteArrayAsync(); var latestRssText = System.Text.Encoding.Default.GetString(RssText);

          if (feed.LatestRssText == latestRssText)
            feed.RunTimeNote = string.Format("{2}{0:N0}kb \t{1:N0}b/ms.", .001 * feed.LatestRssText.Length, feed.LatestRssText.Length / _sw.ElapsedMilliseconds, _same);
          else
          {
            feed.LatestRssText = latestRssText;
            feed.RunTimeNote = string.Format("{2}{0:N0}kb \t{1:N0} b/ms.", .001 * feed.LatestRssText.Length, feed.LatestRssText.Length / _sw.ElapsedMilliseconds, __new);
            feed.LastCheckedAt = _crlnTS;
            feed.LastCheckedPC = Environment.MachineName;
          }
        }
        catch (Exception ex)
        {
          feed.RunTimeNote = ex.Message;
          feed.LatestRssText = ex.ToString();
        }

      return feed;
    }
    async Task<DnLd> updateDnLdFromWebTaskPP(DnLd dnld, HttpClient client, CancellationToken ct) // https://msdn.microsoft.com/en-ca/library/jj155756.aspx
    {
      Debug.WriteLine("Going for dnLd:  {0,4}   {1}", dnld.Id, dnld.CastUrl);

      try
      {        //HttpResponseMessage response = await client.GetAsync(dnLd.CastUrl, ct);        byte[]  RssText = await response.Content.ReadAsByteArrayAsync();        var latestRssText = System.Text.Encoding.Default.GetString(RssText);
        dnld.RunTimeNote = string.Format("### DnLd Launched ###");
        dnld.DnldStatusId = "I"; // Is Being Downloaded
        dnld.ModifiedAt = DateTime.Now;
        dnld.DownloadStart = DateTime.Now;
        await startDownload(dnld);
        dnld.DownloadedAt = DateTime.Now;
      }
      catch (Exception ex)
      {
        dnld.ErrLog = ex.Message;
        dnld.RunTimeNote = ex.ToString();
      }
      return dnld;
    }


    async Task startDownload(DnLd dnld)
    {
      var file = dnld.FullPathFile(MiscHelper.DirPlyr);
      var diry = Path.GetDirectoryName(file);
      if (!Directory.Exists(diry)) Directory.CreateDirectory(diry);

      var wc = new WebClient
      {
        Proxy = WebRequest.DefaultWebProxy //TU: 1/2
      };
      wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials; //TU: 2/2
      wc.BaseAddress = dnld.CastUrl;
      wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
      wc.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(wc_DownloadFileCompleted);
      await wc.DownloadFileTaskAsync(new Uri(dnld.CastUrl), file);

      Debug.WriteLine("{0:HH:mm:ss.fff} - ### Is it done downloading? ###", DateTime.Now);

      dnld.DnldStatusId = "I"; // Is Being Downloaded
      dnld.ModifiedAt = DateTime.Now;
      dnld.DownloadStart = DateTime.Now;
      dnld.DownloadedAt = null;                     //no need here as there is no changes yet: if (dgD != null) Application.Current.Dispatcher.BeginInvoke(new Action(() => dgD.Items.Refresh()));
    }
    async void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      var now = DateTime.Now; if (now < _nextRefreshTime) return; _nextRefreshTime = now.AddMilliseconds(999);

      //Debug.WriteLine("{0:HH:mm:ss.fff} {1,11} / {2,11}  {3,5}%  {4}", DateTime.Now, e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage, ((WebClient)(sender)).BaseAddress);

      var dr = await getDnldRowFromUrl(((WebClient)(sender)).BaseAddress);
      if (dr != null)
        dr.DownloadedLength = e.BytesReceived;
    }
    async void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
      var dr = await getDnldRowFromUrl(((WebClient)(sender)).BaseAddress);
      if (dr == null)
        return;

      if (dr.CastFileLength < dr.DownloadedLength) dr.CastFileLength = dr.DownloadedLength;

      if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
      {
        Appender += $"Oops! Download of {dr.CastTitle} has failed with the error {e.Error.InnermostMessage()}.";
        dr.DnldStatusId = "F"; // Failed to Download
        dr.ErrLog += $"\r\nDnld failed: {e.Error}";
        if (dr.ErrLog.Length > 1020) dr.ErrLog = dr.ErrLog.Substring(0, 1020);  // Trunkate(dr.ErrLog, 1000);
      }
      else
      {
        dr.DnldStatusId = "H"; // HasBeenDownloaded
        dr.ReDownload = false;
        dr.DownloadedAt = DateTime.Now;
        dr.DownloadedByPC = Environment.MachineName;
        dr.DownloadedToDir = MiscHelper.DirPlyr;
        dr.DownloadedLength = new FileInfo(dr.FullPathFile(MiscHelper.DirPlyr)).Length;
        File.SetCreationTime(dr.FullPathFile(MiscHelper.DirPlyr), dr.PublishedAt);
      }

      dr.ModifiedAt = DateTime.Now;

      Bpr.BeepOk();
    }

    static readonly object thisLock = new object();
    DateTime _nextRefreshTime = DateTime.MinValue;

    async Task<DnLd> getDnldRowFromUrl(string url) // 2017 keeps crashing, concurency something.
    {
      try
      {
        lock (thisLock) { return _db.DnLds.FirstOrDefault(r => string.Compare(r.CastUrl, url, true) == 0); } //tu: LINQ to Entities does not recognize the method 'Int32 CompareOrdinal(System.String, System.String)' method, and this method cannot be translated into a store expression.
      }
      catch (Exception ex) { ex.Log(); }
      await Task.Delay(9);
      return null;
    }
  }
}
