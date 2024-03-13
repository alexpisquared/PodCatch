using AsLink;
using Db.PodcastMgt.PowerTools.Models;
using PodcastClientTpl;
using PodcastConditioning;
using PodCatcher.Views;
using System;
using System.Diagnostics;
using System.IO;
using StandardLib.Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using EF.DbHelper.Lib;

namespace PodCatcher.Helpers
{
  internal static class PostDnldHelper
  {
    public static async Task DoPostDownloadProcessing(DnLd dr)
    {
      if (dr == null) return;
      try
      {
        if (Environment.MachineName == "LN1") return;

        if (dr.Feed != null && !string.IsNullOrEmpty(dr.Feed.SubFolderEx)) return; //Dec2014: cut only root folder files.

        dr.DnldStatusId = "C"; //cutting started 
        dr.ReDownload = false;

        if (dr.CastFileLength < dr.DownloadedLength) dr.CastFileLength = dr.DownloadedLength;

        var file = dr.FullPathFile(MiscHelper.DirPlyr);

        if (MiscHelper.IsAudio(file))
        {
          await getUpdateSaveMediaDuration(null, file, dr);

          //var durnMin = await GetMediaDurationInMinUsingMediaElement(file);
          //if (durnMin > 1)
          //	dr.DurationMin = durnMin;
          //else
          //{
          //	if (dr.DurationMin > 240) dr.DurationMin /= 60.0;       // if too big must be in seconds...
          //	if (dr.DurationMin < 2) dr.DurationMin = ConstHelper.Unknown4004Duration;       // if both failed...
          //}

          var OneMinSizeInBytes = (int)(new FileInfo(file).Length / dr.DurationMin);//)) : dr.Feed.KbPerMin * 1024;
          var AdvLengthInBytes = dr.Feed == null ? 0 : (int)(dr.Feed.KbPerMin * 1024 * dr.Feed.AdOffsetSec / 60.0);
          int ttlAudios = 0, ttlDurationMin = 0, durationLeftMin = 0;
          AdvertCutter.OffsetAndCutIntoPieces(dr.Feed == null ? "Lit" : dr.Feed.Name, file, OneMinSizeInBytes, AdvLengthInBytes, ConstHelper._Plyr, ConstHelper._1Cut, dr.PublishedAt, dr.CastTitle, ttlAudios, ttlDurationMin, durationLeftMin,
            dr.Feed.PartSizeMin > 0 ? dr.Feed.PartSizeMin : .75); //3-->2 Oct2011: 3 is too short, too many pieces, etc.
        }
      }
      catch (Exception ex) { ex.Log(); dr.DnldStatusId = "F"; dr.ErrLog = ex.Message; dr.ReDownload = true; }
      finally { if (dr.DnldStatusId != "F") dr.DnldStatusId = "A"; }
    }

    public static async Task<double> GetMediaDurationInMinUsingMediaElement(string fn)
    {
      double durnMin = 1;

      if (Application.Current.Dispatcher.CheckAccess()) // if on UI thread
      {
        durnMin = await new vwMediaPlayer().GetMediaDurationInMinUsingMediaElement(fn);
      }
      else
      {
        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(async () =>
        {
          durnMin = await new vwMediaPlayer().GetMediaDurationInMinUsingMediaElement(fn);
        }));
      }

      return durnMin;
    }
    static (bool needed, string reason) isAnonGenNeeded(string trgDir)
    {
      if (trgDir.Contains(@"_NoAn"))
      {
        return (false, $"####  '_NoAn' means skip the annonce generation.");
      }

      var core = new DirectoryInfo(trgDir).GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => f.Length > 1e6);
      if (core.Count() > 30)
      {
        return (false, $"####  too many files: {core.Count()} (> 30); obviously not for listening in a row.");
      }

      if (core.Average(f => f.Length) < 1e7) // ~10mb ~ 10 min
      {
        return (false, $"####  files are too small: {core.Average(f => f.Length):N0} (< {1e7:N0}).");
      }

      var lastDnloadFileTime = core.Max(f => f.LastWriteTime);
      var hasNew = (DateTime.Now - lastDnloadFileTime).TotalMinutes < 10;

      var ttl = Directory.GetFiles(trgDir, "*.mp3", SearchOption.TopDirectoryOnly).Count();
      var ano = Directory.GetFiles(trgDir, "*.wav.mp3", SearchOption.TopDirectoryOnly).Count();
      var notAll = (ano - 1) * 2 < ttl - 1;

      return (hasNew || notAll, $"####  {trgDir.Split('\\').LastOrDefault(),-32} Latest dnld: {lastDnloadFileTime:yyyy-MM-dd HH} => {(hasNew ? "ToDo" : "Skip")}     Anons/TTL: {ano,4}/{ttl,-4} => {(notAll ? "ToDo" : "Skip")} ==> {((notAll || hasNew) ? "ToDo" : "Skip")} ");
    }

    public static async Task GenerateAllAndFolderAnons(PodcastMgtContext _db, string pathWalkmanMirror)
    {
      try
      {
        var walkmanPlayableFiles = Directory.GetFiles(pathWalkmanMirror, "*.*", SearchOption.AllDirectories).Where(f => MediaHelper.WalkmanPlayableExtDsv.Contains(Path.GetExtension(f).ToLower()) && !f.EndsWith(MediaHelper.AnonsExt));
        var needReqson = isAnonGenNeeded(pathWalkmanMirror);
        if (!needReqson.needed)
        {
          Debug.WriteLine($"<<<<  {pathWalkmanMirror,-80}: {walkmanPlayableFiles.Count(),4} media files,  \t {needReqson.reason}");
        }
        else
        {
          collectTotalTime(_db, walkmanPlayableFiles, out var ttlCasts, out var ttlDurnMin);
          AdvertCutter.CreateSummaryAnons(ttlDurnMin, pathWalkmanMirror);

          Debug.WriteLine($"<<<<  {pathWalkmanMirror,-80}: {walkmanPlayableFiles.Count(),4} media files,  \t {needReqson.reason},  {ttlDurnMin,4:N0}  min ");

          foreach (var file in walkmanPlayableFiles) // add announces for each cast:
          {
            var dr = getDnldRow(_db, file);// Path.GetFileName(dir)); //				
            if (dr == null)
              continue;

            await getUpdateSaveMediaDuration(_db, file, dr);

#if !__DEBUG
            AdvertCutter.CreateOverwriteAnons(dr.Feed == null ? "Unknown feed" : dr.Feed.Name, dr.PublishedAt, dr.CastTitle, ttlCasts, ttlDurnMin, dr.DurationMin.Value, file);
#endif

            ttlDurnMin -= dr.DurationMin.Value;
            ttlCasts--;
            //Bpr.BeepOk();
          }
        }
      }
      catch (Exception ex) { ex.Log(); }
    }

    private static async Task getUpdateSaveMediaDuration(PodcastMgtContext _db, string file, DnLd dr)
    {
      var approximationBySize = (dr.CastFileLength ?? (dr.CastFileLength = new FileInfo(file).Length)) / 1000000;
      if (dr.DurationMin == null ||
        dr.DurationMin.Value < approximationBySize * 0.3 ||
        dr.DurationMin.Value > approximationBySize * 3.0 ||
        dr.DurationMin.Value == ConstHelper.Unknown4004Duration)
      {
        dr.DurationMin = await GetMediaDurationInMinUsingMediaElement(file);

        if (dr.DurationMin == null ||
          dr.DurationMin.Value < approximationBySize * 0.3 ||
          dr.DurationMin.Value > approximationBySize * 3.0 ||
          dr.DurationMin.Value == ConstHelper.Unknown4004Duration)
          dr.DurationMin = approximationBySize;

        if (_db != null)
          await _db.TrySaveReportAsync();
      }

      Debug.Write($"Final: {dr.DurationMin,5:N1}, approx: {approximationBySize,5:N1}, {dr.CastTitle}\n");
    }

    private static void collectTotalTime(PodcastMgtContext _db, System.Collections.Generic.IEnumerable<string> walkmanPlayableFiles, out int ttlCasts, out double ttlDurnMin)
    {
      var af = walkmanPlayableFiles.Where(r => !r.EndsWith(MediaHelper.AnonsExt));
      ttlCasts = af.Count();
      ttlDurnMin = 0.0;
      foreach (var file in af.ToList()) //         foreach (var dir in Directory.GetDirectories(MiscHelper.Dir1Cut))
      {
        var dr = getDnldRow(_db, file);// Path.GetFileName(dir));
        ttlDurnMin += dr == null ? 60 : dr.DurationMin.Value;
      }
    }

    private static async Task doPostDownloadProcessing(PodcastMgtContext _db, System.Collections.Generic.IEnumerable<string> walkmanPlayableFiles)
    {
      foreach (var file in walkmanPlayableFiles)
      {
        var mp3CutDir = Path.Combine(MiscHelper.Dir1Cut, Path.GetFileNameWithoutExtension(file));
        if (!Directory.Exists(mp3CutDir)) // if audio is uncut yet...
        {
          await DoPostDownloadProcessing(getDnldRow(_db, file));
        }

        //Bpr.BeepOk();
      }
    }

    public static string CopyToMp3Player()
    {
      for (var i = (int)'E'; i <= 'Z'; i++)
      {
        var target = $@"{(char)i}:\MUSIC\0pod";
        if (System.IO.Directory.Exists(target))
        {
          var freeMb = new DriveInfo(target.Substring(0, 1)).AvailableFreeSpace / 1000000;
          if (freeMb < 100) return $"Free space is too small:  {freeMb} Mb. Must be not a player connected.";

          Process.Start(new ProcessStartInfo(@"cmd", $@" /k robocopy  C:\Users\alexp\Videos\0Pod\{ConstHelper._PLYR}\  {target}\   /MIR   /XF *.DB *.BAT *.WAV    /XD bbc-wd ch9 BpM   /W:2 "));
          Process.Start(new ProcessStartInfo(Path.Combine(target, ConstHelper._PLYR)));
          return null;
        }
      }

      return $"Directory does not exists. Must be no player connected.";
    }


    static DnLd getDnldRow(PodcastMgtContext db, string audioFile)
    {
      try
      {
        //if (!db.DnLds.Any()) db.DnLds.Load();

        var nmeOnly = Path.GetFileNameWithoutExtension(audioFile);
        var cnt = db.DnLds.Count(r => r.CastFilenameExt.Contains(nmeOnly));
        if (cnt == 1)
          return db.DnLds.First(r => r.CastFilenameExt.Contains(nmeOnly));
        if (cnt > 1)
        {
          var nameExt = Path.GetFileName(audioFile);

          cnt = db.DnLds.Count(r => r.CastFilenameExt.Contains(nameExt));
          if (cnt == 1)
            return db.DnLds.First(r => r.CastFilenameExt.Contains(nameExt));
          if (cnt > 1)
          {
            cnt = db.DnLds.Count(r => r.CastTitle == nmeOnly);
            if (cnt >= 1)
              return db.DnLds.First(r => r.CastTitle == nmeOnly);
            //else // if (cnt > 1)
            //{
            //	MessageBox.Show($"{cnt} matches in BD for \n\n '{nmeOnly}' \n\n\n\t\t\t\t\tWhy???", "You wanna see this!!!", MessageBoxButton.OK, MessageBoxImage.Question);
            //}
          }
        }
        else // == 0
        {
          if (File.Exists(audioFile))
          {
            var dr = new DnLd ///////////////// Not a podcast: found in the folder => add to DB for further reference.
            {
              CastSummary = "Not a podcast: found in the folder.",
              CastFileLength = new FileInfo(audioFile).Length,
              CastFilenameExt = Path.GetFileName(audioFile),
              CastTitle = Path.GetFileNameWithoutExtension(audioFile),
              CastUrl = audioFile,
              DnldStatusId = "C",
              ReDownload = false,
              FeedId = 180,
              Feed = null,
              IsDownloading = false,
              PublishedAt = DateTime.Today,
              ModifiedAt = DateTime.Now,
              RowAddedAt = DateTime.Now,
              //RowAddedByPC = Environment.MachineName,
              ErrLog = "",
              DurationMin = ConstHelper.Unknown4004Duration, //? will be refined later?
              TrgFileSortPrefix = "",
               DnldStatusId_ex = "",
                RunTimeNote = ".",
                SrchD = "."
            };

            db.DnLds.Add(dr);

            /*await*/ db.TrySaveReportAsync();
            return dr;
          }
        }
      }
      catch (Exception ex)
      {
        ex.Log();
      }
      return null;
    }
  }
}
