using StandardLib.Extensions;
using Db.PodcastMgt.PowerTools.Models;
using PodCatcher.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using WebScrap;

namespace PodCatcher
{
  public partial class RssHelper
  {
    static readonly object thisLock = new object();
    public List<RssDnldInfo> RssDnldInfos = new List<RssDnldInfo>();

    public static RssHelper DoFeed(Feed feed)
    {
      Debug.WriteLine("feed url :>> {0}", feed.Url);

      var inst = new RssHelper();

      inst.DnldRssAndParseToGetAllCasts(feed);

      return inst;
    }

    public void DnldRssAndParseToGetAllCasts(Feed feed) { if (!dnldAndParse_New(feed)) dnldAndParse_Old(feed); }

    bool dnldAndParse_New(Feed feed)
    {
      using (var reader = new MyXmlReader(feed.Url))//			var x = XmlReader.Create(feed.Url);
      {
        return p_New(feed, reader);
      }
    }
    void dnldAndParse_Old(Feed feed)
    {
      var feedXML = WebScraper.GetXml(feed.Url);

      p_Old(feed, feedXML);
    }

    bool p_New(Feed feed, MyXmlReader reader)
    {
      RssDnldInfos.Clear();

      try
      {
        var synFeed = SyndicationFeed.Load(reader);         //msdn sample leftover: var rssFormatter = synFeed.GetRss20Formatter();			var rssWriter = new XmlTextWriter("rss.xml", Encoding.UTF8);			rssWriter.Formatting = Formatting.Indented;			rssFormatter.WriteTo(rssWriter);			rssWriter.Close();

        //Application.Current.Dispatcher.BeginInvoke(new Action(() => { feedListBox.ItemsSource = synFeed.Items; loadFeedButton.Content = "Refresh Feed"; })); // In Windows Phone OS 7.1, WebClient events are raised on the same type of thread they were called upon. For example, if WebClient was run on a background thread, the event would be raised on the background thread.  While WebClient can raise an event on the UI thread if called from the UI thread, a best practice is to always use the Dispatcher to update the UI. This keeps the UI thread free from heavy processing. //Deployment.Current.Dispatcher.BeginInvoke(() =>

        //Debug.WriteLine(string.Format("\n\nfeed:>> [{0}] \t {1} \t Total {2} items", feed.Name, feed.Url, synFeed.Items.Count()));
        foreach (var si in synFeed.Items) doSyndItem(feed, si);
        return true;
      }
      catch (Exception ex) { ex.Log($"\n\nfeed>> Name:[{feed.Name}] \t Url:[{feed.Url}] \t "); }

      return false;
    }
    void p_Old(Feed feed, XDocument feedXML)
    {
      lock (thisLock)
      {
        try
        {
          RssDnldInfos.Clear();
          var errMsgs = " ";
          if (feedXML == null)
          {
            feed.LastAvailCastCount = -7;
            Debug.WriteLine(
                $"Unable to get data from  {feed.Url}:\n{System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType.Name}\n{System.Reflection.MethodInfo.GetCurrentMethod().Name}\a");
          }
          else
          {
            var podcastSyndItems = from cast in feedXML.Descendants("item") select cast;
            feed.LastAvailCastCount = podcastSyndItems.Count();
            feed.LastCastAt = DateTime.Today.AddYears(-10);
            var neverRanYet = true;
            foreach (var si in podcastSyndItems)
            {
              //Debug.WriteLine(string.Format("\n{0}\n", feed));

              if (si.Element("pubDate") == null)
              {
                var msg =
                    $"No \"pubDate\" element \n  in podcast: \t {(si.Element("title") != null ? si.Element("title").Value : "")} \n  from feed: \t {feed.Name} \n  feed  url:  \t {feed.Url}";
                Debug.WriteLine(msg);
                errMsgs += msg; // if (showMessages) showMessages = (MessageBox.Show(msg, "Continue showing these messages?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                continue;
              }

              var pubDate = MiscHelper.GetDateFromAnyFormatString(si.Element("pubDate").Value);

              if (feed.LastCastAt < pubDate)
                feed.LastCastAt = pubDate;

              var enclosure = MiscHelper.gerRightEleName(si);
              if (enclosure == "" || si.Element(enclosure).Attribute("url") == null || si.Element(enclosure).Attribute("length") == null)
              {
                if ((DateTime.Now - pubDate).TotalDays > feed.AcptblAgeDay || pubDate < feed.IgnoreBefore)
                  continue;//ignore old errors
                else
                {
                  var msg =
                      $"No \"enclosure\" element \n  in podcast: \t {(si.Element("title") != null ? si.Element("title").Value : "")} \n  published: \t {pubDate} \n  from feed: \t {feed.Name} \n  feed  url:  \t {feed.Url}";
                  Debug.WriteLine(msg);
                  Clipboard.SetText(feed.Url);
                  if (neverRanYet)
                  {
                    neverRanYet = false;
                    Process.Start(new ProcessStartInfo(feed.Url));
                  }
                  else
                    Debug.WriteLine("::>>Second try.");

                  errMsgs += msg; // 	if (showMessages) showMessages = (MessageBox.Show(msg, "Continue showing these messages?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);

                  continue;
                }
              }

              var castTitle = si.Element("title") == null ? "NoTitle" : si.Element("title").Value.Replace("\n", " ").Trim();

              var castUrl = si.Element(enclosure).Attribute("url").Value;

              long castLen = -1;
              long.TryParse(si.Element(enclosure).Attribute("length").Value, out castLen);

              var fnm = MiscHelper.GetSmartPathFileName(castUrl, pubDate, feed.Tla, feed.SubFolderEx, feed.IsTitleInFilename ? castTitle : null, feed.IsNewerFirst);

              RssDnldInfos.Add(new RssDnldInfo
              {
                CastTitle = castTitle,
                Published = pubDate,
                OrgSrcUrl = castUrl,
                AltSrcUrl = "",
                OrignLink = "",
                CastSumry = "old rss parser (xml-based)",
                TtlMinuts = 44.444,
                CastFileLen = castLen,
                CasFilename = fnm
              }); // saveToDnldRow_IsNewAdded(_db, feed, castTitle, pubDate, castUrl, "altSrcUrl", "origLink_", "summary__", 44.444, castLen, fnm);
            }
          } // foreach (var pCast in availablePodcasts)
        }
        catch (Exception ex) { ex.Log(); throw; }
      }
    }

    void doSyndItem(Feed feed, SyndicationItem si)
    {
      //Debug.WriteLine("  Pub Date \t Title:  \t{0} \t\t {1} ", si.PublishDate, si.Title.Text);
      try
      {
        string preSrcUrl = null, fileSize = null;

        var content = si.ElementExtensions.FirstOrDefault(r => r.OuterName == "content");
        if (content != null)
          RssHelper.getFromContent(content, ref preSrcUrl, ref fileSize);

        var orgSrcUrl = preSrcUrl ?? si.Id;
        var altSrcUrl = RssHelper.tryGetStrVal(si, "origEnclosureLink") ?? "";
        var orignLink = RssHelper.tryGetStrVal(si, "origLink") ?? "";
        var duration_ = RssHelper.tryGetStrVal(si, "duration") ?? "44.4";
        var summary_0 = RssHelper.tryGetStrVal(si, "summary") ?? si.Summary.Text;
        var castSumry = summary_0.Length < 1000 ? summary_0 : summary_0.Substring(0, 1000);

        //if (altSrcUrl != "")					orgSrcUrl = altSrcUrl; //for DNR - much faster src... actually, not really.

        //foreach (SyndicationElementExtension see in si.ElementExtensions) { var xe = see.GetObject<XElement>(); Debug.WriteLine("   xEl  =   name/value:  {0,-64}=  {1}", xe.Name, xe.Value); foreach (var atr in xe.Attributes()) Debug.WriteLine("     atr -               {0,-64} - {1}", atr.Name, atr.Value); }
        var ts = TimeSpan.FromMinutes(44);
        if (duration_.Contains(':'))
          TimeSpan.TryParse(duration_, out ts);
        else
          if (double.TryParse(duration_, out var durSec)) ts = TimeSpan.FromSeconds(durSec);

        var enclosure = si.Links.FirstOrDefault(r => r.RelationshipType == "enclosure");

        var fileLen = fileSize != null ? Convert.ToInt64(fileSize) : enclosure != null ? enclosure.Length : 0;
        if (fileLen < 150000 && feed.Name.Contains("CBC")) //cbc seems to be using kb.
          fileLen *= 1024;

        var prefix = feed.Tla;
        if (!isMedia(orgSrcUrl))
        {
          if (enclosure != null && Path.GetExtension(enclosure.Uri.ToString()).Length != 0)
            orgSrcUrl = enclosure.Uri.ToString();
          else
            prefix = "Non-Media\\";
        }

        if (orgSrcUrl.EndsWith("/")) return;

        var castTitle = UnicodeBuster(si.Title.Text).Replace("\n", " ").Trim();
        var fileName = MiscHelper.GetSmartPathFileName(orgSrcUrl, si.PublishDate.Date, prefix, feed.SubFolderEx, feed.IsTitleInFilename ? castTitle : null, feed.IsNewerFirst);

        RssDnldInfos.Add(new RssDnldInfo
        {
          CastTitle = castTitle,
          Published = si.PublishDate.Date > DateTime.Today.AddYears(-100) ? si.PublishDate.Date : DateTime.Today.AddYears(-100),
          OrgSrcUrl = orgSrcUrl,
          AltSrcUrl = altSrcUrl,
          OrignLink = orignLink,
          CastSumry = UnicodeBuster(castSumry),
          TtlMinuts = ts.TotalMinutes,
          CastFileLen = fileLen,
          CasFilename = fileName
        }); // var isNewDlRowAdded = saveToDnldRow_IsNewAdded(_db, feed, si.Title.Text.Replace("\n", " ").Trim(), si.PublishDate.Date, orgSrcUrl_, altSrcUrl, origLink_, summary__, ts.TotalMinutes, fileLen, fnm);
      }
      catch (Exception ex) { ex.Log(); }
    }

    private static string UnicodeBuster(string freeText)
    {
      var ne = @"жйчяабвгдезиклмнопрстуфхцьыъйхЖЙЧЯАБВГДЕЗИКЛМНОПРСТУФХЦЫЖХ";
      var en = @"wjxqabvgdeziklmnoprstufxc'y`jhWJXQABVGDEZIKLMNOPRSTUFXCYJH";
      var ec = en.ToCharArray();

      var ic = ne.ToCharArray().Intersect(freeText.ToCharArray());
      if (ic.Count() <= 0)
        return freeText;

      var sb = new StringBuilder();
      for (var i = 0; i < freeText.Length; i++)
      {
        var c = freeText.Substring(i, 1);
        if (ne.Contains(c))
          sb.Append(ec[ne.IndexOf(c)]);
        else if (c[0] == 'ш') sb.Append("w");
        else if (c[0] == 'щ') sb.Append("w");
        else if (c[0] == 'э') sb.Append("e");
        else if (c[0] == 'ю') sb.Append("u");
        else if (c[0] == 'ì') sb.Append("i");
        else if (c[0] == 'ї') sb.Append("i");
        else if (c[0] == 'Ш') sb.Append("W");
        else if (c[0] == 'Щ') sb.Append("W");
        else if (c[0] == 'Э') sb.Append("E");
        else if (c[0] == 'Ю') sb.Append("U");
        else if (c[0] == 'I') sb.Append("I");
        else if (c[0] == 'є') sb.Append("e");
        else if (c[0] == 'ё') sb.Append("e");
        else if (c[0] > '·')
          sb.Append("_");
        else
          sb.Append(c);
      }

      return sb.ToString();
    }

    static bool isMedia(string orgSrcUrl)
    {
      if (orgSrcUrl == null) return false;
      var ext = Path.GetExtension(orgSrcUrl);
      if (ext.Length == 0) return false;
      if (string.Compare(ext, ".htm", true) == 0) return false;
      if (string.Compare(ext, ".html", true) == 0) return false;

      return true;
    }
    static void getFromContent(SyndicationElementExtension content, ref string orgSrcUrl, ref string fileSize_)
    {
      orgSrcUrl = tryGetAtr(content, "url");
      fileSize_ = tryGetAtr(content, "fileSize");
      //var contType_ = tryGetAtr(content, "type");
    }
    static string tryGetAtr(SyndicationElementExtension content, string s)
    {
      var atr = content.GetObject<XElement>().Attributes().FirstOrDefault(r => r.Name == s);
      return atr == null ? null : atr.Value;
    }
    static string tryGetStrVal(SyndicationItem si, string s)
    {
      var f = si.ElementExtensions.FirstOrDefault(r => r.OuterName.Contains(s));
      var altSrcUrl = f == null ? null : f.GetObject<XElement>().Value;
      return altSrcUrl;
    }
  }
}
