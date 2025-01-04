using System;
using System.IO;

namespace PodcastClientTpl
{
  public class ConstHelper // ORG RO
  {
    public const string
            _0Pod_ = "0Pod",
            _PLYR_ = "_Player",
            _Mirr_ = "_Mirror",
            _Plyr = @$"{_0Pod_}\{_PLYR_}",
#if DEBUG
    _Mirr = $@"{_Plyr}\DevDbg",
#else
    _Mirr = $@"{_Plyr}\{_Mirr_}",
#endif

            _1Cut = @$"{_0Pod_}\Cuts",
            _Lbry = @"\Videos\",
            _AllSrc = @"", // @"1\pod\",
            _VideoTrg = @"1\v\pod\";
    public const double Unknown4004Duration = 40.00004;

    public const string _feedList = @"C:\0\0\WebScrape\PodcastReceiver\FeedList.txt",
#if DEBUG
 _ignoreList = @"C:\0\0\WebScrape\PodcastClient\IgnoreList.Dbg.txt";
#else
 _ignoreList = @"C:\0\0\WebScrape\PodcastClient\IgnoreList.txt";
#endif

    public static string PodRoot
    {
      get
      {
#if REVERSE_TO_REWRITE // does not work .. maybe for installed app will.
//				const string f = @"\\ln1\" +_AllSrc;
				const string f = @"C:\" +_AllSrc;
				if (IsoStUsrStg.UserSettings.Instance.RootFolder != f)
				{
					IsoStUsrStg.UserSettings.Instance.RootFolder = f;
					IsoStUsrStg.UserSettings.Instance.Persist();
				}
#endif
        //Debug.WriteLine(Environment.MachineName);
        //Debug.WriteLine(Environment.UserDomainName);
        //Debug.WriteLine(Environment.UserName);

        string root =
          (Environment.MachineName == "ASUS2" ?
              $"D:\\Users\\{Environment.UserName}{_Lbry}" :
              $"C:\\Users\\{Environment.UserName}{_Lbry}");

        //var myPictures = Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);

        string root0Pod = root + _0Pod_;

        if (!Directory.Exists(root0Pod))
          Directory.CreateDirectory(root0Pod);

        return root;
      }
    }

    public static string Dir1Cut { get { return PodRoot + _1Cut; } }

  }
}
