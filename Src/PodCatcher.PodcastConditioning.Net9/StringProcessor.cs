using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PodcastConditioning
{
  public static class StringProcessor
  {
    public static string MakeSpeakable(string mp3file)
    {
      var s = Path.GetFileNameWithoutExtension(mp3file).Substring(6).Replace("-", " ").Replace("_", " ");

      s = s.Replace("Fdip", "FDIP");
      s = s.Replace("dotnetrocks", "DNR");
      s = s.Replace("runasradio", "run as radio");
      s = s.Replace("WTP", "WTP ");
      s = s.Replace("scia", "Science in Action ");
      s = s.Replace("WTPpodcast", "WTP podcast");
      s = s.Replace("digitalp", "digital planet");
      s = s.Replace("techtalk", "New York Times - Tech Talk");
      s = s.Replace("deepfriedbytes", "deep fried bytes");
      s = s.Replace("ww0", "Windows Weekly");
      s = s.Replace(" 0", " ");
      s = s.Replace(" 0", " ");
      //			s = s.Replace("", "");

      s = new Regex(@"<[^>]*>").Replace(s, " "); //?

      s = Regex.Replace(s, @"([^ ])(\.)([A-z])", "$1$2 $3"); // abc.efg => abc. efg

      //s = Regex.Replace(s, "([a-z])([A-Z])", "$1 $2");			
      //If you use "(\p{Ll})(\p{Lu})" it will pick up accented characters as well.
      //If your strings can contain acronyms you may want to use this:
      s = Regex.Replace(s, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0"); // So "DriveIsSCSICompatible" becomes "Drive Is SCSI Compatible"
      s = Regex.Replace(s, @"([A-z])([0-9])", "$1 $2"); // So "abc123" ==> "abc 123"
      s = Regex.Replace(s, @"([0-9])([A-z])", "$1 $2"); // So "123abc" ==> "123 abc"

      s = processWords(s);
      s = s.Replace("  ", " ").Trim();
      return s;
    }

    static string processWords(string s)
    {
      try
      {
        var rv = "";
        foreach (var word in s.Split(' '))
        {
          if (string.IsNullOrEmpty(word))
            continue;

          rv = processDigiDate(rv, word);
          rv += " ";
        }

        return rv;
      }
      catch (Exception ex)
      {
        Trace.WriteLine(
            $"{ex.Message}\n{System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name}\n{System.Reflection.MethodInfo.GetCurrentMethod().Name}\a");
      }

      return s;
    }
    static string processDigiDate(string rv, string word)
    {
      try
      {
        if (word.Length == 6 && int.TryParse(word, out var ld) && 10100 < ld && ld < 129999)
        {
          var y = int.Parse(word.Substring(4, 2));
          var m = int.Parse(word.Substring(0, 2));
          var d = int.Parse(word.Substring(2, 2));
          var dt = new DateTime(y, m, d);
          rv += dt.ToString("MMMM d");
        }
        else if (word.Length == 8 && int.TryParse(word, out ld) && 20091231 < ld && ld < 22220101)
        {
          var m = int.Parse(word.Substring(4, 2));
          var d = int.Parse(word.Substring(6, 2));
          var y = int.Parse(word.Substring(0, 4));
          var dt = new DateTime(y, m, d);
          rv += dt.ToString("MMMM d");
        }
        else
        {
          rv += word + " ";
        }
      }
      catch (Exception ex)
      {
        Trace.WriteLine(
            $"{ex.Message}\n{System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name}\n{System.Reflection.MethodInfo.GetCurrentMethod().Name}\a");
      }

      return rv;
    }
  }
}
