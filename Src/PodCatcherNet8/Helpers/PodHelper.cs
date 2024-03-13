using System;
using System.IO;
using Db.PodcastMgt.PowerTools.Models;

namespace PodCatcher.Helpers
{
    public class PodHelper
  {
    public static bool IsDownloadRequired(DnLd dnld, ref string rsn)
    {
      if (dnld.ReDownload)                                                      /**/  { rsn = "marked for redownload"; return true; }

      if (dnld.DownloadedAt != null)                                            /**/  { rsn = "Marked as already done (no need to check file system, since it is deleted after use anyway)"; return false; } // .
      if (containsKnownTags(dnld))                                              /**/  { rsn = "contains known tags"; return true; } // 
      if (dnld.Feed == null)                                                    /**/  { rsn = "dnLd.Feed == null"; return false; } // 
      if (dnld.Feed.IsDeleted || !dnld.Feed.IsActive)                           /**/  { rsn = "Feed is IsDeleted OR is not IsActive"; return false; } // 
      if (string.IsNullOrEmpty(dnld.CastUrl.Trim()))                            /**/  { rsn = "URL is missing!!! "; return false; } //todo: log somewhere null url casts
      if (dnld.PublishedAt < dnld.Feed.IgnoreBefore)                            /**/  { rsn =
          $"Published on {dnld.PublishedAt:yyyy-MM-dd} - before allowed date {dnld.Feed.IgnoreBefore:yyyy-MM-dd}."; return false; }
      if ((DateTime.Now - dnld.PublishedAt).TotalDays > dnld.Feed.AcptblAgeDay) /**/  { rsn =
          $"Age {(DateTime.Now - dnld.PublishedAt).TotalDays} exceeds acceptable {dnld.Feed.AcptblAgeDay} days."; return false; }

      var fn = dnld.FullPathFile(MiscHelper.DirPlyr);
      if (existsAndGoodLength(fn, dnld.CastFileLength))
      {
        if (dnld.DownloadedAt == null)
          dnld.DownloadedAt = new FileInfo(fn).LastWriteTime;
        rsn = "File exists and is of Good Length [missing DownloadedAt is set to file date]";
        return false;
      }
      else
      {
        rsn = "File does not exist or is too short";
        return true;
      }
    }

    static bool containsKnownTags(DnLd dnld)
    {
      var kt = Settings.Default.TagCsv.ToLower().Split(',');

      foreach (var t in kt)
      {
        if (dnld.PublishedAt >= dnld.Feed.IgnoreBefore)
          if ((dnld.CastTitle.ToLower().Contains(t) || dnld.CastSummary.ToLower().Contains(t)))
            if (dnld.ErrLog == null || !dnld.ErrLog.ToLower().Contains("error: (404) not found"))
            {
              dnld.Note = $"'cause contains '{t}'.";
              return true;
            }
      }

      return false;
    }

    static bool existsAndGoodLength(string fn, long? len)
    {
      if (!File.Exists(fn))
        return false;
      //setFileDates(fn, pu.PubDate);

      var fi = new FileInfo(fn);
      if (len == fi.Length || len - fi.Length < 100000)
      {
        return true;
      }
      else
      {
        //pu.Story += string.Format("Exists but BAD length Mb: exp-d {0:N1} - actual {1:N1} = {2:N1} ",len / (1048576.0), (fi.Length) / (1048576.0), (len - fi.Length) / (1048576.0));
        return false;
      }
    }
  }
}
