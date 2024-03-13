using AsLink;
using PodcastClientTpl;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using StandardLib.Extensions;

namespace PodCatcher
{
  public class MiscHelper // NEW
  {
    public static string GetSmartPathFileName(string castUrl, DateTime fi, string prefix, string subdir, string title, bool? IsNewerFirst)
    {
      var fnEx = getFilenameExtFromURL(castUrl);
      var pref = (string.IsNullOrWhiteSpace(prefix) ? "" : prefix) + ((MiscHelper.IsAudio(fnEx) && IsNewerFirst == true) ? MiscHelper.GetReverseTimeOrderPrefix(fi) : ""); // only for audio && !subfoldered
      var spfn = (string.IsNullOrWhiteSpace(subdir)) ? (pref + fnEx) : System.IO.Path.Combine(subdir, pref + fnEx);

      if (Path.GetExtension(spfn).Length == 0)
        spfn += ".[NoExt].txt";

      if (!string.IsNullOrEmpty(title))
      {
        if (title.Length > 128)
          title = title.Substring(0, 128); // May 2019.

        spfn = spfn.Replace(Path.GetFileNameWithoutExtension(spfn), pref + safeFileName(title));
      }
      //Debug.WriteLine(string.Format("~~GetSmart: castUrl:{0}, fi:{1}, prefix:{2}, subfolder:{3}, => {4}", castUrl, fi, prefix, subdir, spfn));

      return spfn;
    }
    public static bool IsAudio(string filename)
    {
      var ext = Path.GetExtension(filename);
      return ext.Length > 0 ? (".m4a.mp3.wma.wav".Contains(ext.ToLower())) : false;
    }


    public const string _feedList = @"C:\0\0\WebScrape\PodcastReceiver\FeedList.txt",
#if DEBUG
 _ignoreList = @"C:\0\0\WebScrape\PodcastClient\IgnoreList.Dbg.txt";
#else
 _ignoreList = @"C:\0\0\WebScrape\PodcastClient\IgnoreList.txt";
#endif

    public static string DirRoot => ConstHelper.PodRoot + ConstHelper._0Pod;
    public static string DirPlyr => ConstHelper.PodRoot + ConstHelper._Plyr;
    public static string DirPlr2 => ConstHelper.PodRoot + ConstHelper._Plr2;
    public static string Dir1Cut => ConstHelper.PodRoot + ConstHelper._1Cut;

    static string safeFileName(string str) => str.Replace(":", ";").Replace("+", "-")
        .Replace("|", "-")
        .Replace("\\", "-")
        .Replace("/", "-")
        .Replace("\n", "  ")
        .Replace("\r", "  ")
        .Replace("\t", "  ")
        .Replace("?", "-")
        .Replace(">", ")")
        .Replace("<", "(")
        .Replace("\"", "'")
        .Replace("*", "-");

    public static DateTime GetDateFromAnyFormatString(string str)
    {
      if (DateTime.TryParse(str, out var dt))
        return dt;

      var strDate = str.Replace("EDT", "").Replace("EST", "").Replace("PST", "").Replace("CDT", "").Replace("PDT", "").Replace("CST", "").Replace("Mon,", "").Replace("Tue,", "").Replace("Wed,", "").Replace("Thu,", "").Replace("Thur,", "").Replace("Fri,", "").Replace("Sat,", "").Replace("Sun,", "");

      if ((!string.IsNullOrEmpty(strDate) && DateTime.TryParse(strDate, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out dt)))
        return dt;

      //http://msdn.microsoft.com/en-us/library/w2sa9yss.aspx
      //ing strDate = "Sun 15 Jun 2008 8:30 AM -06:00";
      string[] format = {
                          "dd MMM yyyy HH:mm:ss z",
                          "dd MMM yyyy HH:mm:ss zzz",
                          "dd MMM yyyy HH:mm:ssZ",
                          "dd MMM yyyy HH:mm:ss Z",
                          "dd MMM yyyy HH:mm:ss ZZZ",
                          "dd MMM yyyy HH:mm:ss UTC",
        "ddd dd MMM yyyy h:mm tt zzz",
        "dddd, dd MMM yyyy h:mm tt zzz",

        "dddd, dd MMM yyyy h:mm:ss tt zzz",
        "dddd, dd MMM yyyy h:mm:ss tt zzzz",
        "dddd, dd MMM yyyy h:mm:ss tt zzzzz",
        "dddd. dd MMM yyyy h:mm:ss tt zzz",
        "dddd. dd MMM yyyy h:mm:ss tt zzzz",
        "dddd. dd MMM yyyy h:mm:ss tt zzzzz",
        "dddd\\, dd MMM yyyy h:mm:ss tt zzz",
        "dddd\\, dd MMM yyyy h:mm:ss tt zzzz",
        "dddd\\, dd MMM yyyy h:mm:ss tt zzzzz",

        "dddd, dd MMM yyyy h:mm:sszzz",
        "dddd, dd MMM yyyy h:mm:sszzzz",
        "dddd, dd MMM yyyy h:mm:sszzzzz",
        "dddd. dd MMM yyyy h:mm:sszzz",
        "dddd. dd MMM yyyy h:mm:sszzzz",
        "dddd. dd MMM yyyy h:mm:sszzzzz",
        "dddd\\, dd MMM yyyy h:mm:sszzz",
        "dddd\\, dd MMM yyyy h:mm:sszzzz",
        "dddd\\, dd MMM yyyy h:mm:sszzzzz",

        "dddd, dd MMM yyyy h:mm:ss zzz",
        "dddd, dd MMM yyyy h:mm:ss zzzz",
        "dddd, dd MMM yyyy h:mm:ss zzzzz",
        "dddd. dd MMM yyyy h:mm:ss zzz",
        "dddd. dd MMM yyyy h:mm:ss zzzz",
        "dddd. dd MMM yyyy h:mm:ss zzzzz",
        "dddd\\, dd MMM yyyy h:mm:ss zzz",
        "dddd\\, dd MMM yyyy h:mm:ss zzzz",
        "dddd\\, dd MMM yyyy h:mm:ss zzzzz",

          "dddd, dd MMM yyyy hh:mm:sszzz",
          "dddd, dd MMM yyyy hh:mm:sszzzz",
          "dddd, dd MMM yyyy hh:mm:sszzzzz",
          "dddd. dd MMM yyyy hh:mm:sszzz",
          "dddd. dd MMM yyyy hh:mm:sszzzz",
          "dddd. dd MMM yyyy hh:mm:sszzzzz",
        "dddd\\, dd MMM yyyy hh:mm:sszzz",
        "dddd\\, dd MMM yyyy hh:mm:sszzzz",
        "dddd\\, dd MMM yyyy hh:mm:sszzzzz",

          "dddd, dd MMM yyyy hh:mm:ss zzz",
          "dddd, dd MMM yyyy hh:mm:ss zzzz",
          "dddd, dd MMM yyyy hh:mm:ss zzzzz",
          "dddd. dd MMM yyyy hh:mm:ss zzz",
          "dddd. dd MMM yyyy hh:mm:ss zzzz",
          "dddd. dd MMM yyyy hh:mm:ss zzzzz",
        "dddd\\, dd MMM yyyy hh:mm:ss zzz",
        "dddd\\, dd MMM yyyy hh:mm:ss zzzz",
        "dddd\\, dd MMM yyyy hh:mm:ss zzzzz"
      };
      for (var i = 0; i < format.Length; i++)
      {
        try
        {
          var b = DateTime.TryParseExact(strDate, format[i], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dt); // Console.WriteLine("{0} converts to {1}.", strDate, result.ToString());
          Debug.WriteLine("SUCCESS!!! Match #{0} is found for {1}", i, strDate);
          return dt;
        }
        catch (FormatException ex)
        {
          Debug.WriteLine("{0} is not in the correct format. {1}", strDate, ex.Message);
        }
        catch (Exception ex) { ex.Log(); }
      }

      Debug.WriteLine("FAILURE!!! Match was found for \t\n\t{0}", strDate, "");

      //MessageBox.Show(strDate, "Unable to parse date");
      if (Debugger.IsAttached) Debugger.Break();

      return DateTime.Today.AddYears(-20);
    }
    public static string gerRightEleName(XElement pCast)
    {
      foreach (var enc in new string[] { "enclosure", "podcastmediaenclosure" })
        if (pCast.Element(enc) != null)
          return enc;

      return "";
    }

    static string GetReverseTimeOrderPrefix(DateTime fi)
    {
      var Jan1_2015 = 49310; // => gives range from 2007 to 2035
      return ($"{Jan1_2015 - fi.ToOADate() - 1:000#},`");
    }
    static string getFilenameExtFromURL(string castUrl)
    {
      var podFileName = System.IO.Path.GetFileNameWithoutExtension(castUrl).Replace("%20", " ");
      var podFileExtn = System.IO.Path.GetExtension(castUrl).Replace("%20", " ");

      if (podFileName.Contains("?")) podFileName = podFileName.Split('?')[0];
      if (podFileExtn.Contains("?")) podFileExtn = podFileExtn.Split('?')[0];

      const int maxLen = 44;
      if (podFileName.Length > maxLen)
        podFileName = podFileName.Substring(0, maxLen); // long filenames get out of the sort order in MP3 player.

      var simplPathFile = podFileName + podFileExtn;
      return simplPathFile;
    }
  }
}
