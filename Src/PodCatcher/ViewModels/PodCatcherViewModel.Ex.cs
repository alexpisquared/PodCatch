using AAV.Sys.Ext;
using AAV.Sys.Helpers;
using AsLink;
using Db.PodcastMgt.DbModel;
using PodCatcher.Helpers;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using xMvvmMin;

namespace PodCatcher.ViewModels
{
  partial class PodCatcherViewModel
  {
    void reFilter(string filter)
    {
      var filtered = db.DnLds.Where(r => r.CastTitle.ToLower().Contains(filter) || r.CastSummary.ToLower().Contains(filter)).Take(100);
      InfoApnd = $"{filtered.Count()} matches for '{filter}'";
      FilteredDnLds.ClearAddRangeAuto(filtered);
    }
    async Task reLoad()
    {
      var sw = Stopwatch.StartNew();
      try
      {
        //Db.DnLds.Where(o => (DateTime.Now - o.PublishedAt).TotalDays < 10).OrderByDescending(o => o.PublishedAt).Load(); //loading all for the sake of order.

        var dnlds = Db.DnLds.Where(r => r.IsStillOnline == true).OrderByDescending(o => o.PublishedAt);
        Debug.WriteLine("{0} rows from {1}", dnlds.Count(), dnlds.ToString());
        dnlds.Load(); //loading all for the sake of order.

        var feeds = Db.Feeds.Where(r => /*string.Compare(r.HostMachineId, Environment.MachineName, true) == 0 &&*/ !r.IsDeleted 
#if _DEBUG
        && r.IsActiveDbg
          //)					.Include(d => d.DnLds//?? no go	.OrderByDescending(                 o => o.PublishedAt)
          //both good, but overriden by feed compare:	//
          // .Where                 (r => r.ReDownload || (r.DownloadedAt == null && r.PublishedAt > new DateTime(2013, 11, 22) && string.Compare(r.Feed.HostMachineId , Environment.MachineName, true) == 0)				)
          //).Where(q => q.DnLds.Any(r => r.ReDownload || (r.DownloadedAt == null && r.PublishedAt > new DateTime(2013, 11, 22) && string.Compare(r.Feed.HostMachineId , Environment.MachineName, true) == 0))
          //).Where(q => q.DnLds.Any(r => r.IsStillOnline == true)
          //.Where                 (r => r.IsStillOnline == true) 
#endif
        ).OrderBy(r => r.Name);
        feeds.Load();
        var feedsSql = feeds.ToString();

        //refilterLoaclToFiltered();
        //onLoadExistingAudioFiles(null); Debug.WriteLine("::>Loaded in {0:s\\.ff} sec: {1}, {2},   {3}, {4}, filt.dnld: {5}", sw.Elapsed, Db.Feeds.Count(), Db.Feeds.Count(), Db.DnLds.Count(), Db.DnLds.Count(), CurrentDnLds.Count);
        //await Task.Delay(1000); onLoadExisting(null); Debug.WriteLine("::>Loaded in {0:s\\.ff} sec: {1}, {2},   {3}, {4}, filt.dnld: {5}", sw.Elapsed, Db.Feeds.Count(), Db.Feeds.Count(), Db.DnLds.Count(), Db.DnLds.Count(), CurrentDnLds.Count); Bpr.Beep(999, 29);
        //await Task.Delay(1000); onLoadExisting(null); Debug.WriteLine("::>Loaded in {0:s\\.ff} sec: {1}, {2},   {3}, {4}, filt.dnld: {5}", sw.Elapsed, Db.Feeds.Count(), Db.Feeds.Count(), Db.DnLds.Count(), Db.DnLds.Count(), CurrentDnLds.Count); Bpr.Beep(999, 29);
        //await Task.Delay(1000); onLoadExisting(null); Debug.WriteLine("::>Loaded in {0:s\\.ff} sec: {1}, {2},   {3}, {4}, filt.dnld: {5}", sw.Elapsed, Db.Feeds.Count(), Db.Feeds.Count(), Db.DnLds.Count(), Db.DnLds.Count(), CurrentDnLds.Count); Bpr.Beep(999, 29);

        InfoApnd =
            $"Loaded in {sw.Elapsed:s\\.ff} sec. glb/lcl feed,dlnl: {feeds.Count()} / {Db.Feeds. Count()},  {"dnlds.Count()"}/{Db.DnLds. Count()},  filt.dnld:{CurrentDnLds.Count}";
      }
      catch (Exception ex) { ex.Log(); InfoApnd +=
          $"::>Loaded in {sw.Elapsed:s\\.ff} sec: {ex.Message}"; }

      await Task.Delay(10);
      Bpr.BeepOk();
    }
    void refilterLoaclToFiltered()
    {
      var sw = Stopwatch.StartNew();
      var matches = Db.DnLds.Local
        .Where(r => r.ReDownload || (r.DownloadedAt == null && r.PublishedAt > new DateTime(2013, 11, 22) /*&& string.Compare(r.Feed.HostMachineId, Environment.MachineName, true) == 0*/))
        .OrderByDescending(r => r.PublishedAt)
        .Take(_maxRows);
      Debug.WriteLine($"{"Dnlds filtered"} - Loaded in {sw.Elapsed:s\\.f} sec, matches {matches.Count()}");

      CurrentDnLds.ClearAddRangeAuto(matches);
    }

    public Grid Pnl1 { get { return _pnl1; } set { _pnl1 = value; } }
    Grid _pnl1 = null;
    void checkAllFeeds(object pnl1)
    {
      if (_pnl1 == null)
        _pnl1 = (Grid)pnl1;

      onSaveChanges(null);

      var dgF = _pnl1.FindName("dgF") as DataGrid;
      var dgD = _pnl1.FindName("dgD") as DataGrid;
      var dgH = _pnl1.FindName("dgH") as DataGrid;

      onRefreshDgD(dgD);
      onRefreshDgH(dgH);

      int i = 0;
      foreach (var feed in db.Feeds.Local)
      {
        var t = i * 1000;
        Task.Run(() => Thread.Sleep(t)).ContinueWith(_ =>
        {
          CheckFeedAsync(dgF, feed, dgD);
        }, TaskScheduler.FromCurrentSynchronizationContext());
        i++;
      }
    }
    void CheckFeedAsync(DataGrid dgF, Feed feed, DataGrid dgD)
    {
      Task.Run(() =>
      {
        lock (thisLock) { var i = TtlFeedsToCheck; Interlocked.Increment(ref i); TtlFeedsToCheck = i; }
        CheckFeedSynch(dgF, feed);
      }).ContinueWith(_ =>
      {
        lock (thisLock) { var i = TtlFeedsToCheck; Interlocked.Decrement(ref i); TtlFeedsToCheck = i; }

        if (dgF != null) dgF.Items.Refresh();

        if (TtlFeedsToCheck == 0)
        {
          onSaveChanges(null);

          var unavail = Db.DnLds.Where(r => r.AvailableLastDate != DateTime.Today);
          InfoApnd =
              $"Check of all feeds has finished. loaded {Db.DnLds. Count()} download rows; {unavail.Count()} unavailable; {Db.DnLds. Count() - unavail.Count()} available";
          unavail.ToList().ForEach(r => r.IsStillOnline = false);

          Task.Delay(10000);

          if (IsAutoNextStep)
          {
            if (CurrentDnLds.Count() > 0)
            {
              InfoApnd = $"{DateTime.Now:ddd HH:mm}			Starting {CurrentDnLds.Count()} downloads now.";
              onStartDnlds(dgD);
            }
            else
              InfoApnd = $"{DateTime.Now:ddd HH:mm}			Ready to be closed.";
          }
          else
            InfoApnd =
                $"Check of all feeds has finished; found {CurrentDnLds.Count()} downloads to do. Downloading needs to be started manually!";
        }
      }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    void CheckFeedSynch(DataGrid dgF, Feed feed)
    {
      DoFeed(feed);
      if (dgF != null) Application.Current.Dispatcher.BeginInvoke(new Action(() => { dgF.Items.Refresh(); }));
    }

    void DoFeed(Feed feed)
    {
      feed.StatusInfo = "...";

      var rssFeed = RssHelper.DoFeed(feed);

      saveNewCastsToDb(feed, rssFeed);
    }

    void saveNewCastsToDb(Feed feed, RssHelper rssFeed)
    {
      int newCasts = 0;
      foreach (var rss in rssFeed.RssDnldInfos)
      {
        //Application.Current.Dispatcher.BeginInvoke(new Action(() =>  { // new Func<bool>(() => //tu: new Action(() => 

        DnLd dl;
        if (SaveToDnldRow_IsDnldRequired(feed, rss, out dl))
        {
          Application.Current.Dispatcher.BeginInvoke(new Action(() =>
          {
            if (!CurrentDnLds.Contains(dl))
              CurrentDnLds.Add(dl);
          }));

          newCasts++;

          //? better wait for all feeds check to finish and then move on to dnlding: if (IsAutoNextStep) Dh.StartDownload(db, dl, dgD);					//Dh.LaunchIfDownloadRequiredMvvm(dl, ((DataGrid)dgD));
        }
        //}));
      }
      feed.CastQntNew = newCasts;
      feed.CastQntTtl = rssFeed.RssDnldInfos.Count;
      feed.StatusInfo = $"{newCasts} / {rssFeed.RssDnldInfos.Count}"; //update feed with counts of new casts
      feed.LastCheckedAt = _now;
    }
    bool SaveToDnldRow_IsDnldRequired(Feed feed, RssDnldInfo rss, out DnLd outDnLd) //string title, DateTime pubdate, string orgSrcUrl_, string altSrcUrl, string origLink_, string summary__, double ttlMinutes, long fileLen, string fnm)
    {
      lock (thisLock)
      {
        if (feed.LastCheckedAt != _now) feed.LastCheckedAt = _now;
        if (feed.LastCastAt < rss.Published) feed.LastCastAt = rss.Published;
        //if (db.DnLds.Count()== 0) db.DnLds.Load();

        //r existingLcRow = db.DnLds.FirstOrDefault(r => string.Compare(r.CastTitle, rss.CastTitle, true) == 0 && string.Compare(r.CastFilenameExt, rss.CasFilename, true) == 0); //unique in DB: filename and title ...but 5by5 has changed the filenames 
        var existingLcRow = db.DnLds.FirstOrDefault(r => string.Compare(r.CastUrl, rss.OrgSrcUrl, true) == 0); //unique in DB: as of Jan 2015.
        //if (existingLcRow == null) Aug2016
        //{
        //  db.DnLds.Where(r => string.Compare(r.CastUrl, rss.OrgSrcUrl, true) == 0).Load();
        //  existingLcRow = db.DnLds.FirstOrDefault(r => string.Compare(r.CastUrl, rss.OrgSrcUrl, true) == 0);
        //}
        if (existingLcRow == null)
        {
          var ndr = new DnLd
          {
            CastFileLength = rss.CastFileLen,
            CastFilenameExt = rss.CasFilename,
            CastSummary = safeLen(rss.CastSumry, 1023).Replace("\n\n", Environment.NewLine), // string.IsNullOrEmpty(rss.CastSumry) ? "" : rss.CastSumry.Length >= 1023 ? rss.CastSumry.Substring(0, 1023) : rss.CastSumry,
            CastTitle = rss.CastTitle,
            CastUrl = rss.OrgSrcUrl,
            DnldStatusId = "N",
            FeedId = feed.Id,
            Feed = feed,
            IsDownloading = false,
            PublishedAt = rss.Published,
            ModifiedAt = _now,
            RowAddedAt = _now,
            RowAddedByPC = Environment.MachineName,
            ErrLog = safeLen(string.IsNullOrWhiteSpace(rss.AltSrcUrl) ? "" :
                $"{rss.AltSrcUrl} :alt|org: {rss.OrignLink}", 1000),
            DurationMin = rss.TtlMinuts,
            TrgFileSortPrefix = "",
            AvailableLastDate = DateTime.Today,
            IsStillOnline = true
          };

          db.DnLds.Add(ndr);
          outDnLd = ndr;
          return feed.IsActive && !feed.IsDeleted;
        }
        else // updateExistingDnld:
        {
          UpdateDbRowWhereChanged(rss, existingLcRow);

          outDnLd = existingLcRow;

          return IsDownloadRequiredWrpr(existingLcRow);
        }
      }
    }
    public static void UpdateDbRowWhereChanged(RssDnldInfo rss, DnLd existingRow)
    {
      if (existingRow.CastSummary != rss.CastSumry) { existingRow.CastSummary = safeLen(rss.CastSumry, 1023); existingRow.ErrLog += " Summary Changed+Updated."; if (existingRow.ErrLog.Length > 1020) existingRow.ErrLog = existingRow.ErrLog.Substring(0, 1020); } //  Trunkate(existingRow.ErrLog, 1023); }
      if (existingRow.CastFileLength != rss.CastFileLen) { existingRow.ErrLog +=
          $" File Len:  new {rss.CastFileLen} / {existingRow.CastFileLength} old. "; existingRow.CastFileLength = rss.CastFileLen; if (existingRow.ErrLog.Length > 1020) existingRow.ErrLog = existingRow.ErrLog.Substring(0, 1020); }// Trunkate(existingRow.ErrLog, 1023); }
      if (existingRow.AvailableLastDate != DateTime.Today) { existingRow.AvailableLastDate = DateTime.Today; }
      if (existingRow.IsStillOnline != true) existingRow.IsStillOnline = true;
    }

    public static string safeLen(string s, int len)
    {
      var safeCastSummary = string.IsNullOrEmpty(s) ? "" : s.Length >= len ? s.Substring(0, len) : s;
      return safeCastSummary;
    }
  }
}
