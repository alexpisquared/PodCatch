using AAV.Sys.Ext;
using AAV.Sys.Helpers;
using AsLink;
using Db.PodcastMgt.DbModel;
using MVVM.Common;
using PodCatcher.Properties;
using PodCatcher.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using xMvvmMin;

namespace PodCatcher.ViewModels
{
  public partial class AsyncFineTuningVM : BindableBaseViewModel
  {
    readonly A0DbContext _db = A0DbContext.Create();
    CancellationTokenSource _cts;
    Stopwatch _sw = Stopwatch.StartNew();
    DateTime _crlnTS = DateTime.Now;
    readonly bool _autoStart = false;
    const string _same = "F1 same: ", __new = "F1 ***: ";


    public AsyncFineTuningVM(bool autoStart) => _autoStart = autoStart;
    public void Load() => AutoExec();
    protected override void AutoExec()
    {
      base.AutoExec();
      Bpr.Beep1of2();

      WinTitle = $"PodCatcher - {_crlnTS:ddd HH:mm}";
      DirPlyr = MiscHelper.DirPlyr;
      TagCsv = Settings.Default.TagCsv;
      CurVer = VerHelper.CurVerStr(A0DbContext.SqlEnv);

      try
      {
        //_db.Feeds.Load();
        //_db.DnLds.Load(); //todo: introduces dupes:  .Where(r => r.IsStillOnline == true).Load();

        reLoadFeedList();
        reloadTopNnRecentDnlds();

        Appender = $"{FeedList.Count} / {DnLdList.Count} feeds/dnlds    {(_autoStart ? "Auto staritng ..." : "")} \r\n";

        if (!_autoStart)
          return;

#if __DEBUG
        Task.Run(() => Task.Delay(2000)).ContinueWith(_ =>
        {
          if (Application.Current.Dispatcher.CheckAccess()) // if on UI thread
          {
            addNewFeed(@"http://feeds.feedburner.com/consciousrunnerpodcast?format=xml", "DEV DBG", "DEV DBG", true, DateTime.Today.AddDays(-7));
          }
          else
          {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
              addNewFeed(@"http://feeds.feedburner.com/consciousrunnerpodcast?format=xml", "DEV DBG", "DEV DBG", true, DateTime.Today.AddDays(-7))));
          }

          Bpr.BeepOk();
        }); // , TaskScheduler.FromCurrentSynchronizationContext());
#else
        Task.Run(CountDown(9.1)).ContinueWith(_ =>
        {
          if (IsCntDnOn)
          {
            IsCntDnOn = false;
            onF9All4Steps(null);
            WinTitle = $"PodCatcher - {_crlnTS:ddd HH:mm}";
          }
          //else InfoMsg = "Countdown aborted.\r\n";
        });
#endif
      }
      catch (Exception ex) { ex.Log(); Appender += "\r\nDownloads failed.\r\n" + ex.ToString(); FC = Brushes.Violet; }

      Bpr.Beep2of2();
    }
    protected override bool CanClose() => !IsCntDnOn;

    Func<Task> CountDown(double delaySec) => async () =>
                                                 {
                                                   int fmin = VerHelper.IsMyHomePC ? 7000 : 19000, fadd = 2000, pow = 9, minDur = 50;
                                                   var startTime = DateTime.Now.AddSeconds(delaySec);
                                                   while (IsCntDnOn && DateTime.Now < startTime)
                                                   {
                                                     var secLeft = Math.Abs((startTime - DateTime.Now).TotalSeconds);
                                                     var _0to1 = Math.Abs(Math.Pow(((delaySec - secLeft) / delaySec), pow)); //0 - 1
                                                     var frq = (int)(fmin + fadd * _0to1); //500 - 5500
                                                     var dur = minDur + (int)(1000 * _0to1);
                                                     Appender = $"Launching in {(startTime - DateTime.Now).TotalSeconds:N0} seconds ... Alt-B to aBort  {dur:N1}";
                                                     Bpr.BeepFD(frq, dur);
                                                     //InfoMsg = "";
                                                     await Task.Delay(1000 - dur); Debug.WriteLine("{0} - {1}", pow, dur);
                                                   }

                                                   if (IsCntDnOn)
                                                     Bpr.BeepFD(fmin + fadd, 1000);
                                                 };


    string _cv = "?!@#";            /**/ public string CurVer { get => _cv; set => Set(ref _cv, value); }
    string _SrchF;                  /**/ public string SrchF { get => _SrchF; set { if (Set(ref _SrchF, value) && value.Length > 0) { onSearchF(value); } } }
    string _SrchD;                  /**/ public string SrchD { get => _SrchD; set { if (Set(ref _SrchD, value) && value.Length > 0) { onSearchD(value); } } }
    string _WinTitle = "";          /**/ public string WinTitle { get => _WinTitle; set => Set(ref _WinTitle, value); }
    string _TagCsv = "carb";        /**/ public string TagCsv { get => _TagCsv; set { if (Set(ref _TagCsv, value)) Settings.Default.TagCsv = TagCsv; Settings.Default.Save(); } }
    string _DirPlyr = "";           /**/ public string DirPlyr { get => _DirPlyr; set => Set(ref _DirPlyr, value); }
    string _Appender = "";          /**/ public string Appender { get => _Appender; set { var l = value.Split('\n'); if (l.Length > 6) { value = string.Join("\n", l, 1, l.Length - 1); } Set(ref _Appender, value); } } // value.StartsWith("^") ? value : value.Contains("\n") ? value + this._InfoMsg : this._InfoMsg + value); } }
    bool _IsBusy = false;           /**/ public bool IsBusy { get => _IsBusy; set => Set(ref _IsBusy, value); }
    bool _RowDtlVisModeF = false;   /**/ public bool RowDtlVisModeF { get => _RowDtlVisModeF; set => Set(ref _RowDtlVisModeF, value); }
    bool _RowDtlVisModeD = false;   /**/ public bool RowDtlVisModeD { get => _RowDtlVisModeD; set => Set(ref _RowDtlVisModeD, value); }
    int _Max1 = 9;                  /**/ public int Max1 { get => _Max1; set => Set(ref _Max1, value); }
    int _Val1;                      /**/ public int Val1 { get => _Val1; set => Set(ref _Val1, value); }
    int _Max2;                      /**/ public int Max2 { get => _Max2; set => Set(ref _Max2, value); }
    int _Val2;                      /**/ public int Val2 { get => _Val2; set => Set(ref _Val2, value); }
    int _Max3;                      /**/ public int Max3 { get => _Max3; set => Set(ref _Max3, value); }
    int _Val3;                      /**/ public int Val3 { get => _Val3; set => Set(ref _Val3, value); }
    int _Max4;                      /**/ public int Max4 { get => _Max4; set => Set(ref _Max4, value); }
    int _Val4;                      /**/ public int Val4 { get => _Val4; set => Set(ref _Val4, value); }
    bool _IncDel = false;           /**/ public bool IncDel { get => _IncDel; set { if (Set(ref _IncDel, value)) reLoadFeedList(); } }
    bool _IsFeedNmVsbl = false;     /**/ public bool IsFeedNmVsbl { get => _IsFeedNmVsbl; set => Set(ref _IsFeedNmVsbl, value); }
    bool _canStopCntDn = true;      /**/ public bool IsCntDnOn { get => _canStopCntDn; set => Set(ref _canStopCntDn, value); }
    double _MUProgressPerc = 0;     /**/ public double MUProgressPerc { get => _MUProgressPerc; set => Set(ref _MUProgressPerc, value); }
    double _FeedsPnlHeight = 222;   /**/ public double FeedsPnlHeight { get => _FeedsPnlHeight; set => Set(ref _FeedsPnlHeight, value); }
    TaskbarItemProgressState _mups = TaskbarItemProgressState.None; public TaskbarItemProgressState MUProgressState { get => _mups; set => Set(ref _mups, value); }

    Brush _DF = Brushes.Gray;       /**/ public Brush DF { get => _DF; set => Set(ref _DF, value); }
    Brush _FC = Brushes.Gray;       /**/ public Brush FC { get => _FC; set => Set(ref _FC, value); }
    Brush _DC = Brushes.Gray;       /**/ public Brush DC { get => _DC; set => Set(ref _DC, value); }
    Brush _AG = Brushes.Gray;       /**/ public Brush AG { get => _AG; set => Set(ref _AG, value); }

    ObservableCollection<Feed> _fl; /**/ public ObservableCollection<Feed> FeedList { get => _fl; set => _fl = value; }
    ObservableCollection<DnLd> _dl; /**/ public ObservableCollection<DnLd> DnLdList { get => _dl; set => _dl = value; }

    Feed _SelectFeed;               /**/ public Feed SelectFeed { get => _SelectFeed; set { if (Set(ref _SelectFeed, value)) threadAwareRun(onChgdSelectFeed2); } }
    DnLd _SelectDnLd;               /**/ public DnLd SelectDnLd { get => _SelectDnLd; set { if (Set(ref _SelectDnLd, value)) onChgdSelectDnLd(); } }

    ICommand _F1UpdtFeeds;          /**/ public ICommand F1UpdtFeedsCmd => _F1UpdtFeeds ?? (_F1UpdtFeeds = new RelayCommand(x => onF1UpdtFeeds(x), x => canF1UpdtFeeds) { GestureKey = Key.F1, GestureModifier = ModifierKeys.None });
    ICommand _F2FindNewDL;          /**/ public ICommand F2FindNewDLCmd => _F2FindNewDL ?? (_F2FindNewDL = new RelayCommand(async x => await onF2FindNewDL(x), x => canF2FindNewDL) { GestureKey = Key.F2, GestureModifier = ModifierKeys.None });
    ICommand _F3DnLdCasts;          /**/ public ICommand F3DnLdCastsCmd => _F3DnLdCasts ?? (_F3DnLdCasts = new RelayCommand(x => onF3DnLdCasts(x), x => canF3DnLdCasts) { GestureKey = Key.F3, GestureModifier = ModifierKeys.None });
    ICommand _F4AnonsGenr;          /**/ public ICommand F4AnonsGenrCmd => _F4AnonsGenr ?? (_F4AnonsGenr = new RelayCommand(x => onF4AnonsGenr(x), x => canF4AnonsGenr) { GestureKey = Key.F4, GestureModifier = ModifierKeys.None });
    ICommand _F9All4Steps;          /**/ public ICommand F9All4StepsCmd => _F9All4Steps ?? (_F9All4Steps = new RelayCommand(x => onF9All4Steps(x), x => canF9All4Steps) { GestureKey = Key.F9, GestureModifier = ModifierKeys.None });
    ICommand _CancelAsy;            /**/ public ICommand CancelAsyCmd => _CancelAsy ?? (_CancelAsy = new RelayCommand(x => onCancelAsy(x), x => canCancelAsy) { GestureKey = Key.F6, GestureModifier = ModifierKeys.None });
    ICommand _F6;                   /**/ public ICommand F6Cmd => _F6 ?? (_F6 = new RelayCommand(x => onF6(x), x => canF6) { GestureKey = Key.F6, GestureModifier = ModifierKeys.None });
    ICommand _F7;                   /**/ public ICommand F7Cmd => _F7 ?? (_F7 = new RelayCommand(x => onF7(x), x => canF7) { GestureKey = Key.F7, GestureModifier = ModifierKeys.None });
    ICommand _F8;                   /**/ public ICommand F8Cmd => _F8 ?? (_F8 = new RelayCommand(async x => await onF8(x), x => canF8) { GestureKey = Key.F8, GestureModifier = ModifierKeys.None });
    ICommand _StopCntDn;            /**/ public ICommand StopCntDnCmd => _StopCntDn ?? (_StopCntDn = new RelayCommand(x => onStopCntDn(x), x => true) { GestureKey = Key.Escape, GestureModifier = ModifierKeys.None });
    ICommand _DelCasts;             /**/ public ICommand DelCastsCmd => _DelCasts ?? (_DelCasts = new RelayCommand(x => onDelCasts(x), x => SelectFeed != null) { GestureKey = Key.None, GestureModifier = ModifierKeys.None });
    ICommand _DelCasAl;             /**/ public ICommand DelCasAlCmd => _DelCasAl ?? (_DelCasAl = new RelayCommand(x => onDelCasAl(x), x => SelectFeed != null) { GestureKey = Key.None, GestureModifier = ModifierKeys.None });

    ICommand _DbChck;               /**/ public ICommand DbChckCmd => _DbChck ?? (_DbChck = new RelayCommand(x => onDbChck(x), x => canDbChck) { GestureKey = Key.A, GestureModifier = ModifierKeys.Control });
    ICommand _DbSave;               /**/ public ICommand DbSaveCmd => _DbSave ?? (_DbSave = new RelayCommand(x => onDbSave(x), x => canDbSave) { GestureKey = Key.B, GestureModifier = ModifierKeys.Control });
    ICommand _SeeDLs;               /**/ public ICommand SeeDLsCmd => _SeeDLs ?? (_SeeDLs = new RelayCommand(x => onSeeDLs(x), x => canSeeDLs) { GestureKey = Key.C, GestureModifier = ModifierKeys.Control });
    ICommand _Fd;                   /**/ public ICommand FdCmd => _Fd ?? (_Fd = new RelayCommand(x => onFd(x), x => canFd) { GestureKey = Key.D, GestureModifier = ModifierKeys.Control });
    ICommand _Recent;               /**/ public ICommand RecentCmd => _Recent ?? (_Recent = new RelayCommand(x => onRecent(x), x => canRecent) { GestureKey = Key.E, GestureModifier = ModifierKeys.Control });
    ICommand _Dnlded;               /**/ public ICommand DnldedCmd => _Dnlded ?? (_Dnlded = new RelayCommand(x => onDnlded(x), x => canDnlded) { GestureKey = Key.E, GestureModifier = ModifierKeys.Control });
    ICommand _Pendng;               /**/ public ICommand PendngCmd => _Pendng ?? (_Pendng = new RelayCommand(x => onPendng(x), x => canPendng) { GestureKey = Key.E, GestureModifier = ModifierKeys.Control });

    public bool canF1UpdtFeeds => !IsBusy;
    async void onF1UpdtFeeds(object p) { IsCntDnOn = false; IsBusy = true; Bpr.Beep1of2(); await asy1UpdtFeeds(); IsBusy = false; Bpr.Beep2of2(); await refreshUi(); }
    public bool canF2FindNewDL => !IsBusy;
    async Task onF2FindNewDL(object p) { IsCntDnOn = false; IsBusy = true; Bpr.Beep1of2(); await asy2FindNewDL(); IsBusy = false; Bpr.Beep2of2(); await refreshUi(); }
    public bool canF3DnLdCasts => !IsBusy;
    async void onF3DnLdCasts(object p) { IsCntDnOn = false; IsBusy = true; Bpr.Beep1of2(); await asy3UpdtDnLds(); IsBusy = false; Bpr.Beep2of2(); await refreshUi(); }
    public bool canF4AnonsGenr => !IsBusy;
    async void onF4AnonsGenr(object p) { IsCntDnOn = false; IsBusy = true; Bpr.Beep1of2(); await asy4AnonsGenr(); IsBusy = false; Bpr.Beep2of2(); await refreshUi(); }
    public bool canF9All4Steps => !IsBusy;
    async void onF9All4Steps(object p) { IsCntDnOn = false; IsBusy = true; await asyAll4Steps(); IsBusy = false; await refreshUi(); }
    public bool canCancelAsy => IsBusy;
    void onCancelAsy(object p) { Bpr.Beep1of2(); if (_cts != null) { _cts.Cancel(); } Bpr.Beep2of2(); }
    public bool canF6 => !IsBusy;
    public bool canF7 => IsBusy;
    public bool canF8 => !IsBusy;

    void onStopCntDn(object p)
    {
      Bpr.BeepClk();
      if (IsCntDnOn)
        IsCntDnOn = false;
      else
      {
        if (!_db.ChangeTracker.Entries().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
          CloseAppCmd.Execute(p);
        else
        {
          retry:
          switch (MessageBox.Show(_db.GetDbChangesReport(22), "Save Changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes))
          {
            case MessageBoxResult.Yes: var rowsSaved = _db.TrySaveReport().rowsSavedCnt; if (rowsSaved < 0) goto retry; CloseAppCmd.Execute(p); break;
            case MessageBoxResult.No: CloseAppCmd.Execute(p); break;
            case MessageBoxResult.Cancel: return;
          }
        }
      }
    }
    void onDelCasts(object p)
    {
      Bpr.BeepClk();
      var remove = SelectFeed.DnLds.Where(r => r.AvailableLastDate == DateTime.Today);
      //remove.ToList().ForEach(r => DnLdList.Remove(r)); 
      _db.DnLds.RemoveRange(remove);
      DnLdList.ClearAddRangeAuto(SelectFeed.DnLds);
      info();
    }
    void onDelCasAl(object p) { Bpr.BeepClk(); _db.DnLds.RemoveRange(SelectFeed.DnLds); DnLdList.Clear(); info(); }
    void onF6(object p)
    {
      var unavail = _db.DnLds.Where(r => r.AvailableLastDate != DateTime.Today); //          
      Appender =
          $"^loaded {_db.DnLds.Count()} download rows; {unavail.Count()} unavailable; {_db.DnLds.Count() - unavail.Count()} available";
      unavail.ToList().ForEach(r => r.IsStillOnline = false);
    }
    void onF7(object p) => IsBusy = false;
    async Task onF8(object p)
    {
      IsBusy = true;
      Bpr.Beep1of2();
      await Task.Delay(22);      //foreach (Feed f in FeedList) { f.RunTimeNote += DateTime.Now.ToString("HH:mm:ss"); };

      try
      {
        var dlg = new vwAddFeed
        {
          Feed = preCreateNewFeed(DateTime.Now)
        };
        dlg.ShowDialog();
        if (dlg.DialogResult == true)
          addNewFeed(dlg.Feed);
      }
      catch (Exception ex) { ex.Log(); }

      Bpr.Beep2of2();
      IsBusy = false;
    }

    void addNewFeed(Feed feed)
    {
      if (!_db.Machines.Any(r => string.Compare(r.Id, Environment.MachineName, true) == 0))
        _db.Machines.Add(new Machine { Id = Environment.MachineName, IsUsable = true, TargetDrive = "C", Note = $"Auto added on {DateTime.Now}" });

      FeedList.Add(_db.Feeds.Add(feed));
    }
    void addNewFeed(string url, string name, string note, bool isActive, DateTime Since)
    {
      var now = DateTime.Now;

      if (!_db.Machines.Any(r => string.Compare(r.Id, Environment.MachineName, true) == 0))
        _db.Machines.Add(new Machine
        {
          Id = Environment.MachineName,
          IsUsable = true,
          TargetDrive = "C",
          Note =
            $"Auto added on {now}"
        });

      FeedList.Add(_db.Feeds.Add(preCreateNewFeed(url, name, note, isActive, Since, now)));
    }

    static Feed preCreateNewFeed(DateTime now) => new Feed
    {
      //Url = url,
      //Name = name,
      //Note = note,
      //IsActive = isActive,
      Tla = "tla-",
      SubFolder = "_Player",
      IsTitleInFilename = true,
      IsNewerFirst = false,
      StatusInfo = "...",
      AcptblAgeDay = 7,
      AddedAt = now,
      AdOffsetSec = 0,
      HostMachineId = Environment.MachineName,
      IgnoreBefore = now.AddDays(-7),
      KbPerMin = 44,
      LastAvailCastCount = 0,
      LastCheckedAt = now,
      //LastCastAt = now,
      LastCheckedPC = Environment.MachineName,
      PartSizeMin = .5
    };
    static Feed preCreateNewFeed(string url, string name, string note, bool isActive, DateTime Since, DateTime now) => new Feed
    {
      Url = url,
      Name = name,
      Note = note,
      IsActive = isActive,
      IsTitleInFilename = true,
      IsNewerFirst = false,
      StatusInfo = "...",
      AcptblAgeDay = 7,
      AddedAt = now,
      AdOffsetSec = 0,
      HostMachineId = Environment.MachineName,
      IgnoreBefore = Since,
      KbPerMin = 44,
      LastAvailCastCount = 0,
      LastCheckedAt = now,
      LastCastAt = now,
      LastCheckedPC = Environment.MachineName,
      PartSizeMin = .5,

    };

    public bool canDbChck => !IsBusy;
    public bool canDbSave => !IsBusy;
    bool canSeeDLs;

    public bool canFd => !IsBusy;
    public bool canRecent => !IsBusy;
    public bool canDnlded => !IsBusy;
    public bool canPendng => !IsBusy;

    void onDbChck(object p = null) { IsBusy = true; Bpr.BeepShort(); Appender += _db.GetDbChangesReport(); IsBusy = false; }
    void onDbSave(object p = null) { IsBusy = true; Bpr.BeepShort(); Appender += _db.TrySaveReport("r/s DB Saved. \r\n"); IsBusy = false; }
    void onSeeDLs(object p = null) { Bpr.BeepShort(); SelectFeed = null; }
    void onFd(object p) { IsBusy = true; Bpr.Beep1of2(); Appender = $"{FeedList?.Count} {DateTime.Now}"; refreshUiSynch(); IsBusy = false; Bpr.Beep2of2(); }
    void onRecent(object p) { Bpr.BeepShort(); SelectFeed = null; reloadTopNnRecentDnlds(); } // reloadActiveRecentDnlds(DateTime.Now.AddDays(-1)); 
    void onDnlded(object p) { Bpr.BeepShort(); SelectFeed = null; reloadTopNnDnldedDnlds(); } // reloadActiveRecentDnlds(DateTime.Now.AddDays(-1)); 
    void onPendng(object p) { Bpr.BeepShort(); SelectFeed = null; reloadTopNnPendngDnlds(); } // reloadActiveRecentDnlds(DateTime.Now.AddDays(-1)); 

    void threadAwareRun(Action actn)
    {
      var s = $" ** {(Application.Current.Dispatcher.CheckAccess() ? "On UI." : "Darn! this is not UI thread any more")} ";
      Debug.WriteLine(s);
      new System.Speech.Synthesis.SpeechSynthesizer().SpeakAsync(s);

      if (Application.Current.Dispatcher.CheckAccess()) // if on UI thread
      {
        actn();
      }
      else
      {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => actn()));
      }
    }
    void onChgdSelectFeed2()
    {
      if (IsCntDnOn)
        IsCntDnOn = false;

      IsFeedNmVsbl = (SelectFeed == null);

      if (SelectFeed == null)
        reloadTopNnRecentDnlds();
      else
      {
        try
        {
          var sw = Stopwatch.StartNew();

          // UNOMMENTING THIS RENDERS NEXT LINE NOT CALLABLE ???? // Apr2019.
          //Task.Run(async () =>
          //{
          //  await asy1UpdtFeeds(new List<Feed> { SelectFeed });
          //  await asy2FindNewDL(new List<Feed> { SelectFeed });
          //  new System.Speech.Synthesis.SpeechSynthesizer().Speak("1");
          //}).Wait();
          //new System.Speech.Synthesis.SpeechSynthesizer().Speak("2");

          DnLdList.ClearAddRangeAuto(_db.DnLds.Where(r => r.FeedId == SelectFeed.Id).OrderByDescending(r => r.PublishedAt).ToList()); // local 5 times faster but has only handful of records

          info(sw.Elapsed);
        }
        catch (Exception ex) { ex.Log(); }

        canSeeDLs = true;
      }

    }
    void onChgdSelectDnLd() { }
  }
}
/* WX: Dec 2015:
https://s.ch9.ms/Series/Developers-Guide-to-Windows-10-Preview/feed/mp4
https://s.ch9.ms/Series/A-Developers-Guide-to-Windows-10/feed/mp4high
  */
