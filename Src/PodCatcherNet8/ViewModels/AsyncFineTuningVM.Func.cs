using StandardLib.Extensions;
using AsLink;
using Db.PodcastMgt.PowerTools.Models;
using PodCatcher.Views;
using PodCatcherNet8.AsLink;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using StandardLib.Helpers;
using AmbienceLib;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EF.DbHelper.Lib;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PodCatcher.Helpers;

namespace PodCatcher.ViewModels
{
  public partial class AsyncFineTuningVM
  {
    async Task asy1UpdtFeeds(List<Feed> feeds = null)
    {
      MUProgressPerc += .06; MUProgressState = TaskbarItemProgressState.Normal;

      if (feeds == null) feeds = FeedList.ToList();

      //todo: var feeds2 = FeedList.Select(r => r);

      _crlnTS = DateTime.Now;
      _sw = Stopwatch.StartNew();
      DF = Brushes.Blue;

      _cts = new CancellationTokenSource();
      try
      {
        Appender = "F1. Updating Feeds ... ";
        await asyUpdtFeedsCT(_cts.Token, feeds); //////////////////////////////////////////////////////////////////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        Appender += "complete. \r\n"; //        InfoMsg += DbSaveLib.TrySaveReport(db) + "  Updating Feeds ... complete.";

        _cts = null;
        IsBusy = false;
        DF = Brushes.LightGreen;
        MUProgressPerc += .1; MUProgressState = TaskbarItemProgressState.Paused;
      }
      catch (OperationCanceledException ex) { ex.Log(); Appender += "\r\nDownloads canceled.\r\n"; DF = Brushes.Violet; }
      catch (Exception ex) { ex.Log(); Appender += "\r\nDownloads failed.\r\n" + ex.ToString(); DF = Brushes.Violet; }
    }
    async Task asy2FindNewDL(List<Feed> feedsParam = null)
    {
      MUProgressPerc += .06; MUProgressState = TaskbarItemProgressState.Normal;

      var feeds = feedsParam ?? FeedList.Where(r => r.RunTimeNote.StartsWith(__new)).ToList();        //var lst = FeedList.Where(r => r.Name == "CBC Ideas "); //.RunTimeNote.StartsWith(__new));

      Max2 = feeds.Sum(r => r.DnLds.Where(d => d.IsStillOnline == true).Count());

      try
      {
        FC = Brushes.Blue;

        Appender += $"F2. Checking {feeds.Count}{(feedsParam == null ? " NEW" : " ALL")} feeds for new casts ... ";

#if !AwaitNotWorking
        await Task.Delay(3);
        foreach (var feed in feeds.ToList())
        {
          try
          {
            Debug.WriteLine(feed.Name);
            var ri = RssHelper.FindNewCasts(feed)?.RssDnldInfos;
            Debug.WriteLine("~> {0,-32}   {1,5} casts found in RSS", feed.Name, ri.Count);
            await addUPdate_SetIdrFlag(feed, ri);
            //Val2++;
          }
          catch (Exception ex) { ex.Log(); feed.RunTimeNote = ex.Message; FC = Brushes.Violet; }
        }

        //WARNING!!! This one does not AWAIT!!! ->lst.ToList().ForEach(async feed => { try { await addUPdate_SetIdrFlag(feed, RssHelper.FindNewCasts(feed).RssDnldInfos); } catch (Exception ex) { ex.Log(); feed.RunTimeNote = ex.Message; FC = Brushes.Violet; } });

        Appender += $"done.  ({Val2} / {Max2} / {feeds.Sum(r => r.DnLds.Where(d => d.IsStillOnline == true).Count())})"; //saving must be donw only after dnldg finished otherwise there will be omissions from F2Same case:        InfoMsg += DbSaveLib.TrySaveReport(_db, "Db-saving new found casts done. \r\n");

        //recursive calling itwself: onChgdSelectFeed1();

        if (feeds.Count == 1)
          reload1(feeds[0]);
        else
          reloadActiveRecentDnlds();

        FC = Brushes.LightGreen;
#else
        var t = Task.Run(() => FeedList.ToList().ForEach(feed =>
        {
          try { addUPdate_SetIdrFlag(feed, RssHelper.FindNewCasts(feed).RssDnldInfos); }
          catch (Exception ex) { ex.Log(); feed.RunTimeNote = ex.Message; FC = Brushes.Violet; }
        }));

        await t.ContinueWith(_ =>
        {
          InfoMsg += DbSaveLib.TrySaveReport(_db, "Db-Saving new casts");

          FC = Brushes.LightGreen;
        }, TaskScheduler.FromCurrentSynchronizationContext());

        InfoMsg += "\r\nWaiting ...";

        t.Wait();
#endif
      }
      catch (Exception ex) { ex.Log(); Appender += "\r\nasy2FindNewDL() failed.\r\n" + ex.ToString(); FC = Brushes.Violet; }
      MUProgressPerc += .09; MUProgressState = TaskbarItemProgressState.Paused;
    }
    async Task asy3UpdtDnLds()
    {
      MUProgressPerc += .06; MUProgressState = TaskbarItemProgressState.Normal;
      _sw = Stopwatch.StartNew();
      _crlnTS = DateTime.Now;
      DC = Brushes.Blue;
      _cts = new CancellationTokenSource();
      try
      {
        Appender += $"F3. Downloading {FeedList.Sum(r => r.NewCastCount)} new casts ... ";
        await asyUpdtDnLdsCT(_cts.Token); //////////////////////////////////////////////////////////////////<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        Appender += "done. ";

        SelectFeed = null; // <== show dnld list.

        _cts = null;
        IsBusy = false;
        DC = Brushes.LightGreen;
      }
      catch (OperationCanceledException ex) { ex.Log(); Appender += "\r\nDownloads canceled.\r\n"; DC = Brushes.Violet; }
      catch (Exception ex) { ex.Log(); Appender += "\r\nDownloads failed.\r\n" + ex.ToString(); DC = Brushes.Violet; }
      MUProgressPerc += .09; MUProgressState = TaskbarItemProgressState.Paused;

    }
    public async Task asy4AnonsGenr()
    {
      MUProgressPerc += .06; MUProgressState = TaskbarItemProgressState.Normal;
      try
      {
        AG = Brushes.Blue;
        Appender += "F4. Splitting and annons generation ... ";
        Val4 = 33;

        foreach (var path in Directory.GetDirectories(MiscHelper.DirPlyr, "*.*", SearchOption.TopDirectoryOnly))
          await PostDnldHelper.GenerateAllAndFolderAnons(_db, path);

#if !DEBUG
        PostDnldHelper.CopyToMp3Player();
#endif

        Appender += "done. \r\n";
        Val4 = 100;
        AG = Brushes.LightGreen;
      }
      catch (Exception ex) { ex.Log(); Appender += "\r\nDownloads failed.\r\n" + ex.ToString(); AG = Brushes.Violet; }
      MUProgressPerc += .09; MUProgressState = TaskbarItemProgressState.Paused;
    }
    async Task asyAll4Steps()
    {
      try
      {
        DF = FC = DC = AG = Brushes.LightGray;
        MUProgressPerc = 0; MUProgressState = TaskbarItemProgressState.Normal;

        //Bpr.BeepFDNK(2000, 100, 20);
#if DEBUG_THIS
        Bpr.BeepDone();
      await asy4AnonsGenr();
#else
        await asy1UpdtFeeds();
        await asy2FindNewDL();
        onDbSave();
        await asy3UpdtDnLds();
        onDbSave();
        await asy4AnonsGenr();
        await asy2FindNewDL(FeedList.Where(r => !r.IsDeleted).ToList());
        onDbSave(null);
#endif

        //RecentCmd.Execute(null);
      }
      finally
      {
        MUProgressPerc = 1; MUProgressState = TaskbarItemProgressState.Paused;
        //Bpr.BeepDone();
      }
    }

    async Task addUPdate_SetIdrFlag(Feed feed, List<RssDnldInfo> rdis)
    {
      try
      {
        feed.RunTimeNote = $"{rdis.Count:N0} casts in RSS. ";
        feed.NewCastCount = 0;
        foreach (var rdi in rdis.OrderByDescending(r => r.Published))
        {
          var dnld = await dnLd_AddOrUpdate(feed, rdi);
          if (dnld != null)
          {
            //dnld.ReDownload = PodCatcherViewModel.IsDownloadRequiredWrpr(dnld);
            if (dnld.ReDownload) feed.NewCastCount++;
          }
          Val2++;//Appender += "·";          Bpr.BeepFD(14000, 55);
        }
        feed.CastQntNew = feed.NewCastCount;
        feed.CastQntTtl = rdis.Count;
        feed.StatusInfo = $"{feed.NewCastCount} / {rdis.Count}"; //update feed with counts of new casts
      }
      catch (Exception ex) { ex.Log(); throw; }
    }

    async Task<DnLd> dnLd_AddOrUpdate(Feed feed, RssDnldInfo rss)
    {
      //await Task.Delay(3); //todo: resolve:   A second operation started on this context before a previous asynchronous operation completed. Use 'await' to ensure that any asynchronous operations have completed before calling another method on this context. Any instance members are not guaranteed to be thread safe.
      try
      {
        var existingLcRow = await _db.DnLds.FirstOrDefaultAsync(r => string.Compare(r.CastUrl, rss.OrgSrcUrl, true) == 0); // ??        _db.DnLds.FirstOrDefault(r => string.Compare(r.CastUrl, rss.OrgSrcUrl, true) == 0);    //cast url is unique in DB (as of Jan 2015)
        if (existingLcRow == null)
        {
          var dnld = new DnLd
          {
            CastFileLength = rss.CastFileLen,
            CastFilenameExt = rss.CasFilename,
            //CastSummary = PodCatcherViewModel.safeLen(rss.CastSumry, 2048).Replace("\n\n", Environment.NewLine), // string.IsNullOrEmpty(rss.CastSumry) ? "" : rss.CastSumry.Length >= 1023 ? rss.CastSumry.Substring(0, 1023) : rss.CastSumry,
            CastTitle = rss.CastTitle,
            CastUrl = rss.OrgSrcUrl,
            DnldStatusId = "N",
            FeedId = feed.Id,
            Feed = feed,
            IsDownloading = false,
            PublishedAt = rss.Published,
            ModifiedAt = _crlnTS,
            RowAddedAt = _crlnTS,
            //RowAddedByPC = Environment.MachineName,
            //ErrLog = PodCatcherViewModel.safeLen(string.IsNullOrWhiteSpace(rss.AltSrcUrl) ? "" :                $"{rss.AltSrcUrl} :alt|org: \r\n{rss.OrignLink}", 1000),
            DurationMin = rss.TtlMinuts,
            TrgFileSortPrefix = "",
            AvailableLastDate = DateTime.Today,
            IsStillOnline = true,
            DownloadedAt = null,
            RunTimeNote = "New  DnLd row",
            DnldStatusId_ex = "",
            SrchD = "",

          };

          _db.DnLds.Add(dnld);

          return dnld;
        }
        else
        {
          //PodCatcherViewModel.UpdateDbRowWhereChanged(rss, existingLcRow);
          return existingLcRow;
        }
      }
      catch (Exception ex) { ex.Log(); }
      return null;
    }


    void reload1(Feed feed, DateTime? upTo = null) { DnLdList = new ObservableCollection<DnLd>(_db.DnLds.Where(r => r.FeedId == feed.Id).OrderByDescending(r => r.PublishedAt)); info(); }
    void reloadActiveRecentDnlds(DateTime? upTo = null) { DnLdList = new ObservableCollection<DnLd>(_db.DnLds.Where(r => (r.ModifiedAt >= (upTo ?? _crlnTS)) || r.ReDownload).OrderByDescending(r => r.ModifiedAt)); IsFeedNmVsbl = true; info(); }
    void reloadTopNnRecentDnlds(int topX = 128)
    {
      //_db.Database.CommandTimeout = 900; // seconds
      DnLdList = new ObservableCollection<DnLd>(_db.DnLds.OrderByDescending(r => r.PublishedAt)
        //too slow: .ThenBy(r => r.Feed.Name)  :2024
        .Take(topX));
      IsFeedNmVsbl = true;
      info();
    }
    void reloadTopNnDnldedDnlds(int topX = 128) { DnLdList = new ObservableCollection<DnLd>(_db.DnLds.Where(r => r.DownloadedAt != null).OrderByDescending(r => r.PublishedAt).ThenBy(r => r.Feed.Name).Take(topX)); IsFeedNmVsbl = true; info(); }
    void reloadTopNnPendngDnlds(int topX = 128) { DnLdList = new ObservableCollection<DnLd>(_db.DnLds.Where(r => r.ReDownload).OrderByDescending(r => r.PublishedAt).ThenBy(r => r.Feed.Name).Take(topX)); IsFeedNmVsbl = true; info(); }
    void onSearchF(string value)
    {
      try
      {
        FeedList = new ObservableCollection<Feed>(_db.Feeds.Where(r =>
          (r.Name != null && r.Name.ToLower().Contains(value)) ||
          (r.Note != null && r.Note.ToLower().Contains(value)) ||
          (r.RunTimeNote != null && r.RunTimeNote.ToLower().Contains(value))
          ).OrderByDescending(r => r.Name));

        FeedList.ToList().ForEach(r => r.SrchF = SrchF);

        info();
      }
      catch (Exception ex) { ex.Log(); Appender += "\r\nSearch failed.\r\n" + ex.ToString(); }
    }
    void onSearchD(string value)
    {
      try
      {
        DnLdList = new ObservableCollection<DnLd>(_db.DnLds.Where(r =>
          (r.CastTitle != null && r.CastTitle.ToLower().Contains(value)) ||
          (r.CastSummary != null && r.CastSummary.ToLower().Contains(value)) ||
          (r.Note != null && r.Note.ToLower().Contains(value)) ||
          (r.ErrLog != null && r.ErrLog.ToLower().Contains(value))
          ).OrderByDescending(r => r.ModifiedAt));

        DnLdList.ToList().ForEach(r => r.SrchD = SrchD);

        IsFeedNmVsbl = true;
        info();
      }
      catch (Exception ex) { ex.Log(); Appender += "\r\nSearch failed.\r\n" + ex.ToString(); }
    }

    void reLoadFeedList()
    {
      FeedList = new ObservableCollection<Feed>();
      foreach (Feed feed in _db.Feeds.Where(r => (IncDel || !r.IsDeleted)).OrderByDescending(r => r.IsActive).ThenBy(r => r.Name))
        FeedList.Add(feed);
      refreshUiSynch();
    }
    void reLoadFeedList_()
    {
      FeedList = new ObservableCollection<Feed>(_db.Feeds.Where(r =>
#if DEBUG
        //r.Id == 101 &&
#endif
        // /*string.Compare(r.HostMachineId, Environment.MachineName, true) == 0 &&*/  --------------- Oct 2016
        (IncDel || !r.IsDeleted)).OrderByDescending(r => r.IsActive).ThenBy(r => r.Name));
    }

    void info(TimeSpan? ts = null) => WinTitle = $"PodCatcher - {_crlnTS:ddd HH:mm} - Feeds:{FeedList.Count}  Dnlds:{DnLdList.Count}  - {(ts == null ? 0 : ts.Value.TotalMilliseconds):N0} ms";
  }
}


/* Oct 2016 - drop unique url req-t:
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
DROP INDEX IX_DnLds ON dbo.DnLds
GO
DROP INDEX IX_DnLds_1 ON dbo.DnLds
GO
ALTER TABLE dbo.DnLds SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

 
	 
	 
	 
	 
	 UPDATE       Feeds SET                IsDeleted = 0  WHERE        (IsDeleted <> 1) AND (HostMachineId <> 'asus2')

 
	 
	 */
