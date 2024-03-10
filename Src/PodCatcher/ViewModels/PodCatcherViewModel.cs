using System;
using System.Data.Entity;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using PodCatcher.Helpers;
using System.IO;
using AsLink;
using System.Threading;
using System.Data;
using System.Speech.Synthesis;
using PodCatcher.Views;
using MVVM.Common;
using Db.PodcastMgt.DbModel;
using xMvvmMin;
using AAV.Sys.Helpers;

namespace PodCatcher.ViewModels
{
    internal partial class PodCatcherViewModel : BindableBaseViewModel//, IPodCatcherViewModel
	{
		public A0DbContext Db { get { return db ?? (db = A0DbContext.Create()); } set { db = value; } }		A0DbContext db;
		SpeechSynthesizer synth = new SpeechSynthesizer();

		CultureInfo _culture = new CultureInfo("es-ES", false);
		const int _maxRows = 50;
		static Object thisLock = new Object();
		static DateTime _now = DateTime.Now;
		bool canGenerateAnoncesProp = true;

		public int TtlFeedsToCheck { get { return _TtlFeedsToCheck; } set { this.Set(ref this._TtlFeedsToCheck, value); } }										int _TtlFeedsToCheck = 0;
		public int TtlDnLds2Finish { get { return _TtlDnLds2Finish; } set { this.Set(ref this._TtlDnLds2Finish, value); } }										int _TtlDnLds2Finish = 0;
		public DateTime SinceDate { get { return _SinceDate; } set { this.Set(ref this._SinceDate, value); } }																DateTime _SinceDate = DateTime.Today.AddDays(-10);
		public bool IsCheckingFeeds { get { return _isCheckingFeeds; } set { this.Set(ref this._isCheckingFeeds, value); } }									bool _isCheckingFeeds = false;
		public bool IsDnLoadinCasts { get { return _isDnLoadinCasts; } set { this.Set(ref this._isDnLoadinCasts, value); } }									bool _isDnLoadinCasts = false;
		public bool IsAutoNextStep { get { return _IsAutoNextStep; } set { this.Set(ref this._IsAutoNextStep, value); } }											bool _IsAutoNextStep = true;
		public bool IsBusyAtAll { get { return _isBusy; } set { this.Set(ref this._isBusy, value); } }																				bool _isBusy = false;
		public Panel LayoutRoot { get { return _LayoutRoot; } set { this.Set(ref this._LayoutRoot, value); } }																Panel _LayoutRoot;
		public string FilterString { get { return _FilterString; } set { this.Set(ref this._FilterString, value); reFilter(value); } }				string _FilterString = "";
		public string InfoApnd { get { return _InfoApnd; } set { this.Set(ref this._InfoApnd, value + "\r\n" + _InfoApnd); } }										string _InfoApnd = "";
		public Feed SelectedFeed { get { return _SelectedFeed; } set { this.Set(ref this._SelectedFeed, value); AvailableDnLds.ClearAddRangeAuto(db.DnLds.Where(r => r.FeedId == value.Id && r.IsStillOnline)); } } Feed _SelectedFeed = null;// InfoApnd = string.Format(" glb/lcl feed,dlnl: {1} / {2},  {3} / {4},    CurrentDnLds:{5}  ...{6}", "", Db.Feeds.Count(), Db.Feeds.Count(), Db.DnLds.Count(), Db.DnLds.Count(), CurrentDnLds.Count, AvailableDnLds.Count());
		public DnLd SelectedDnLd { get { return _SelectedDnLd; } set { this.Set(ref this._SelectedDnLd, value); } }														DnLd _SelectedDnLd = null;
		public ObservableCollection<DnLd> CurrentDnLds { get { return _CurrentDnLds; } }																															readonly ObservableCollection<DnLd> _CurrentDnLds = new ObservableCollection<DnLd>();
		public ObservableCollection<DnLd> FilteredDnLds { get { return _FilteredDnLds; } }																														readonly ObservableCollection<DnLd> _FilteredDnLds = new ObservableCollection<DnLd>();
		public ObservableCollection<DnLd> AvailableDnLds { get { return _AvailableDnLds; } }																													readonly ObservableCollection<DnLd> _AvailableDnLds = new ObservableCollection<DnLd>();

		public string CurVer { get { return _cv; } set { this.Set(ref this._cv, value); } }									string  _cv = "?!@#";

		public PodCatcherViewModel(bool countDown = true)
		{
			Bpr.Beep1of2();

      CurVer = VerHelper.CurVerStr(A0DbContext.SqlEnv);   //bool isDay0; if (bool.TryParse(MiscDataHelper.GetSetting("daq", "IsDay0", "True", "Directive flag to run all tasks in Full or Daily/Delta mode.").SValue, out isDay0))IsDay0 = isDay0;

      IsAutoNextStep = countDown;

			Task.Run(() => Thread.Sleep(1))
				.ContinueWith(async _ => await reLoad(), TaskScheduler.FromCurrentSynchronizationContext())
				.ContinueWith(_ =>
				{
#if !SkipCountDown
					if (IsAutoNextStep) Bpr.BeepOkB(); Thread.Sleep(1000);
					if (IsAutoNextStep) Bpr.BeepFD(9000, 200); Thread.Sleep(1000);
					if (IsAutoNextStep) Bpr.BeepFD(8600, 200); Thread.Sleep(1000);
					if (IsAutoNextStep) Bpr.BeepFD(8300, 200); Thread.Sleep(1000);
					if (IsAutoNextStep) Bpr.BeepFD(8000, 600); Thread.Sleep(1000);
					if (IsAutoNextStep) Bpr.BeepFD(7000, 200); Thread.Sleep(5);
					if (IsAutoNextStep) Bpr.BeepFD(7000, 200);
#endif
				})
				.ContinueWith(_ => { if (IsAutoNextStep) onDoAll(null); }, TaskScheduler.FromCurrentSynchronizationContext())
				.ContinueWith(_ => Bpr.Beep2of2(), TaskScheduler.FromCurrentSynchronizationContext());
		}

		public bool isBusy { get { return IsBusyAtAll = (TtlFeedsToCheck + TtlDnLds2Finish == 0); } }

		public ICommand DoAllCmd { get { return _DoAllCmd ?? (_DoAllCmd = new RelayCommand(x => onDoAll(x), x => isBusy) { GestureKey = Key.A, GestureModifier = ModifierKeys.Alt }); } }		ICommand _DoAllCmd;
		void onDoAll(object pnl1) { IsAutoNextStep = true; checkAllFeeds(pnl1); }

		public ICommand CheckFeedsAsyncCmd { get { return _CheckFeedsAsyncCmd ?? (_CheckFeedsAsyncCmd = new RelayCommand(x => onCheckFeedsAsync(x), x => isBusy)); } }		ICommand _CheckFeedsAsyncCmd;
		void onCheckFeedsAsync(object pnl1) { IsAutoNextStep = false; checkAllFeeds(pnl1); }

		public ICommand ChkSelectFeedCmd { get { return _ChkSelectFeedCmd ?? (_ChkSelectFeedCmd = new RelayCommand(x => onChkSelectFeed(x), x => canChkSelectFeed) { GestureKey = Key.F1, GestureModifier = ModifierKeys.None }); } }		ICommand _ChkSelectFeedCmd;
		public bool canChkSelectFeed { get { return SelectedFeed != null; } }
		void onChkSelectFeed(object pnl1)
		{
			Bpr.Beep1of2();

			var dgF = ((Grid)pnl1).FindName("dgF") as DataGrid;
			var dgD = ((Grid)pnl1).FindName("dgD") as DataGrid;

			CheckFeedSynch(((DataGrid)dgF), SelectedFeed);
			//((DataGrid)dg).Items.Refresh();

			//Db.DnLds.Where(r => r.ReDownload || (r.DownloadedAt == null && r.PublishedAt > new DateTime(2013, 11, 22) && string.Compare(r.Feed.HostMachineId , Environment.MachineName, true) == 0)).OrderByDescending(r => r.PublishedAt).Load();
			//			refilterLoaclToFiltered();
			Bpr.Beep2of2();
		}

		public ICommand CheckSelectedFeedAsyncCmd { get { return _CheckSelectedFeedAsyncCmd ?? (_CheckSelectedFeedAsyncCmd = new RelayCommand(x => onCheckSelectedFeedAsync(x), x => canCheckSelectedFeedAsync) { GestureKey = Key.F3, GestureModifier = ModifierKeys.None }); } }		ICommand _CheckSelectedFeedAsyncCmd;
		public bool canCheckSelectedFeedAsync { get { return SelectedFeed != null; } }
		void onCheckSelectedFeedAsync(object pnl1)
		{
			var dgF = ((Grid)pnl1).FindName("dgF") as DataGrid;
			var dgD = ((Grid)pnl1).FindName("dgD") as DataGrid;

			CheckFeedAsync(dgF, SelectedFeed, dgD);
		}

		public ICommand DeleteAllDnldsOfThisFeedCmd { get { return _DeleteAllDnldsOfThisFeedCmd ?? (_DeleteAllDnldsOfThisFeedCmd = new RelayCommand(x => onDeleteAllDnldsOfThisFeed(x), x => canDeleteAllDnldsOfThisFeed)); } }		ICommand _DeleteAllDnldsOfThisFeedCmd;
		public bool canDeleteAllDnldsOfThisFeed { get { return SelectedFeed != null; } }
		void onDeleteAllDnldsOfThisFeed(object menuZero)
		{
			while (SelectedFeed.DnLds.Count > 0)
			{
				var dl = SelectedFeed.DnLds.FirstOrDefault();
				Db.Entry(dl).State = EntityState.Deleted; //tu: use State - not: SelectedFeed.DnLds.Remove(dl);
			}
		}


		public ICommand RefreshFeedsCmd { get { return _RefreshFeedsCmd ?? (_RefreshFeedsCmd = new RelayCommand(x => onRefreshFeeds(x), x => canRefreshFeeds) { GestureKey = Key.F5, GestureModifier = ModifierKeys.None }); } }		ICommand _RefreshFeedsCmd;
		public bool canRefreshFeeds { get { return true; } }
		void onRefreshFeeds(object dgF)
		{
			Bpr.Beep1of2();
			((DataGrid)dgF).Items.Refresh();
			InfoApnd =
			    $"{"Feed:Db/Lcl, DnLd:Db/Lcl"}  {Db.Feeds.Count()}/{Db.Feeds. Count()},   {Db.DnLds.Count()}/{Db.DnLds. Count()},   filt.dnld: {CurrentDnLds.Count}";
			Bpr.Beep2of2();
		}

		public ICommand RefreshDgDCmd { get { return _RefreshDgDCmd ?? (_RefreshDgDCmd = new RelayCommand(x => onRefreshDgD(x), x => canRefreshDgD) { GestureKey = Key.F6, GestureModifier = ModifierKeys.None }); } }		ICommand _RefreshDgDCmd;
		public bool canRefreshDgD { get { return true; } }
		void onRefreshDgD(object dgD)
		{
			Bpr.Beep1of2();
			Debug.WriteLine("::> {1}, {2},   {3}, {4}, filt.dnld: {5}", "", Db.Feeds.Count(), Db.Feeds.Count(), Db.DnLds.Count(), Db.DnLds.Count(), CurrentDnLds.Count);
			((DataGrid)dgD).Items.Refresh();
			Debug.WriteLine("::> {1}, {2},   {3}, {4}, filt.dnld: {5}", "", Db.Feeds.Count(), Db.Feeds.Count(), Db.DnLds.Count(), Db.DnLds.Count(), CurrentDnLds.Count);
			InfoApnd =
			    $"{"Feed:Db/Lcl, DnLd:Db/Lcl"}  {Db.Feeds.Count()}/{Db.Feeds. Count()},   {Db.DnLds.Count()}/{Db.DnLds. Count()},   filt.dnld: {CurrentDnLds.Count}";
			Bpr.Beep2of2();
		}

		public ICommand RefreshDgHCmd { get { return _RefreshDgHCmd ?? (_RefreshDgHCmd = new RelayCommand(x => onRefreshDgH(x), x => canRefreshDgH) { GestureKey = Key.F7, GestureModifier = ModifierKeys.None }); } }		ICommand _RefreshDgHCmd;
		public bool canRefreshDgH { get { return true; } }
		void onRefreshDgH(object dgH)
		{
			Bpr.Beep1of2();
			Debug.WriteLine("::> {1}, {2},   {3}, {4}, filt.dnld: {5}", "", Db.Feeds.Count(), Db.Feeds.Count(), Db.DnLds.Count(), Db.DnLds.Count(), CurrentDnLds.Count);
			((DataGrid)dgH).Items.Refresh();
			Debug.WriteLine("::> {1}, {2},   {3}, {4}, filt.dnld: {5}", "", Db.Feeds.Count(), Db.Feeds.Count(), Db.DnLds.Count(), Db.DnLds.Count(), CurrentDnLds.Count);
			InfoApnd =
			    $"{"Feed:Db/Lcl, DnLd:Db/Lcl"}  {Db.Feeds.Count()}/{Db.Feeds. Count()},   {Db.DnLds.Count()}/{Db.DnLds. Count()},   filt.dnld: {CurrentDnLds.Count}";
			Bpr.Beep2of2();
		}

		public ICommand StartDnldsCmd { get { return _StartDnldsCmd ?? (_StartDnldsCmd = new RelayCommand(x => onStartDnlds(x), x => canStartDnlds) { GestureKey = Key.F2, GestureModifier = ModifierKeys.Alt }); } }		ICommand _StartDnldsCmd;
		public bool canStartDnlds { get { return true; } }
		void onStartDnlds(object dgD) { foreach (var dl in CurrentDnLds) { LaunchIfDownloadRequiredMvvm(Db, dl, ((DataGrid)dgD)); } }

		public ICommand DnldOneCmd { get { return _DnldOneCmd ?? (_DnldOneCmd = new RelayCommand(x => onDnldOne(x), x => canDnldOne) { GestureKey = Key.D, GestureModifier = ModifierKeys.Alt }); } }		ICommand _DnldOneCmd;
		public bool canDnldOne { get { return SelectedDnLd != null && SelectedDnLd.DnldStatusId != "I"; } }
		void onDnldOne(object dgD)
		{
			Bpr.BeepOk();

			SelectedDnLd.ReDownload = true;

			//Application.Current.Dispatcher.BeginInvoke(new Action(() =>			{
			if (!CurrentDnLds.Contains(SelectedDnLd)) CurrentDnLds.Add(SelectedDnLd);
			//}));

			onStartDnlds(dgD); // StartDownload(SelectedDnLd, ((DataGrid)dgD));
		}

		public ICommand DeleteOneCmd { get { return _DeleteOneCmd ?? (_DeleteOneCmd = new RelayCommand(x => onDeleteOne(x), x => canDeleteOne) { }); } }		ICommand _DeleteOneCmd;
		public bool canDeleteOne { get { return SelectedDnLd != null; } }
		void onDeleteOne(object dgH)
		{
			Bpr.BeepOk();

			//Application.Current.Dispatcher.BeginInvoke(new Action(() =>			{
			if (CurrentDnLds.Contains(SelectedDnLd)) CurrentDnLds.Remove(SelectedDnLd);

			var td = db.DnLds.FirstOrDefault(r => r.Id == SelectedDnLd.Id);
			if (td != null) db.DnLds.Remove(td);

			SelectedDnLd = null;
			//}));
		}

		public ICommand CutOneCmd { get { return _CutOneCmd ?? (_CutOneCmd = new RelayCommand(async x => await onCutOne(x), x => canCutOne) { GestureKey = Key.C, GestureModifier = ModifierKeys.Alt }); } }		ICommand _CutOneCmd;
		public bool canCutOne { get { return SelectedDnLd != null; } }
		async Task onCutOne(object x) { Bpr.BeepOk(); await PostDnldHelper.DoPostDownloadProcessing(SelectedDnLd); }

		public ICommand GenerateAnoncesCmd { get { return _GenerateAnoncesCmd ?? (_GenerateAnoncesCmd = new RelayCommand(async x => await onGenerateAnonces(x), x => canGenerateAnonces) { GestureKey = Key.F9, GestureModifier = ModifierKeys.None }); } }		ICommand _GenerateAnoncesCmd;
		public bool canGenerateAnonces { get { return canGenerateAnoncesProp; } }
		async Task onGenerateAnonces(object x) { Bpr.Beep1of2(); canGenerateAnoncesProp = false; await PostDnldHelper.GenerateAllAndFolderAnons(Db, MiscHelper.DirPlr2); Bpr.Beep2of2(); canGenerateAnoncesProp = true; }

		public ICommand DnldCastCmd { get { return _DnldCastCmd ?? (_DnldCastCmd = new RelayCommand(async x => await onDnldCast(x), x => canDnldCast) { GestureKey = Key.F11, GestureModifier = ModifierKeys.None }); } }		ICommand _DnldCastCmd;
		public bool canDnldCast { get; set; }
		async Task onDnldCast(object x) { Bpr.Beep1of2(); canDnldCast = false; await Task.Delay(9); /*Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)); */Bpr.Beep2of2(); canDnldCast = true; }

		public ICommand LoadExistingCmd { get { return _LoadExistingCmd ?? (_LoadExistingCmd = new RelayCommand(x => onLoadExistingAudioFiles(x), x => canLoadExisting) { GestureKey = Key.Escape, GestureModifier = ModifierKeys.Alt }); } }		ICommand _LoadExistingCmd;
		public bool canLoadExisting { get { return true; } }
		void onLoadExistingAudioFiles(object notUsed)
		{
			if (!Directory.Exists(MiscHelper.DirPlyr)) Directory.CreateDirectory(MiscHelper.DirPlyr);

			foreach (var file in Directory.GetFiles(MiscHelper.DirPlyr))
			{
				if (!".m4a.mp3.wma.wav".Contains(Path.GetExtension(file).ToLower())) continue;

				Debug.WriteLine(file, "Load FS exist-g");

				if (CurrentDnLds.FirstOrDefault(r => file.Contains(r.CastFilenameExt)) != null) continue; // already there

				var t = Db.DnLds.FirstOrDefault(r => file.Contains(r.CastFilenameExt));
				if (t == null)
				{
					Db.DnLds.Where(r => file.Contains(r.CastFilenameExt)).OrderByDescending(r => r.PublishedAt).Load();

					t = Db.DnLds.FirstOrDefault(r => file.Contains(r.CastFilenameExt));
					if (t == null) t = Db.DnLds.FirstOrDefault(r => file.Contains(r.CastFilenameExt));
					if (t == null) Debug.WriteLine("   not in _db yet; strange.");
				}

				if (t != null)
					CurrentDnLds.Add(t);
			}

			Bpr.BeepOk();
		}

		public ICommand CheckSaveCmd { get { return _CheckSaveCmd ?? (_CheckSaveCmd = new RelayCommand(x => onCheckSave(x), x => canCheckSave) { }); } }		ICommand _CheckSaveCmd;
		public bool canCheckSave { get { return true; } }
		void onCheckSave(object x) { AsLink.DbSaveMsgBox_OldRestoredInDec2023.CheckAskSave(Db); }

		public ICommand SaveChangesCmd { get { return _SaveChangesCmd ?? (_SaveChangesCmd = new RelayCommand(x => onSaveChanges(x), x => canSaveChanges) { }); } }		ICommand _SaveChangesCmd;
		public bool canSaveChanges { get { return true; } }
		void onSaveChanges(object x)
		{
			Bpr.BeepOk();
			InfoApnd = $"{db.TrySaveReport().rowsSavedCnt} rows saved";
			Bpr.BeepOk();
		}

		public ICommand AddNewCmd { get { return _AddNewCmd ?? (_AddNewCmd = new RelayCommand(x => onAddNew(x), x => canAddNew) { GestureKey = Key.F8, GestureModifier = ModifierKeys.None }); } }		ICommand _AddNewCmd;
		public bool canAddNew { get { return true; } }
		void onAddNew(object x)
		{
			IsAutoNextStep = false;
			var dlg = new vwAddFeed();
			dlg.ShowDialog();
			if (dlg.DialogResult == true)
			{
				var now = DateTime.Now;

				if (!Db.Machines.Any(r => string.Compare(r.Id, Environment.MachineName, true) == 0))
					Db.Machines.Add(new Machine { Id = Environment.MachineName, IsUsable = true, TargetDrive = "C", Note =
					    $"Auto added on {now}"
					});

				Db.Feeds.Add(new Feed
				{
					Name = dlg.Feed.Name,
					Note = dlg.Feed.Note,
					Url = dlg.Feed.Url,
					IsActive = dlg.Feed.IsActive,
					StatusInfo = "...",
					AcptblAgeDay = 7,
					AddedAt = now,
					AdOffsetSec = 0,
					HostMachineId = Environment.MachineName,
					//IgnoreBefore = dlg.Feed.Since,
					KbPerMin = 44,
					LastAvailCastCount = 0,
					LastCheckedAt = now,
					LastCastAt = now,
					LastCheckedPC = Environment.MachineName,
          PartSizeMin = .5
				});
			}
		}
	}

	//public static class ObservableCollectionEx
	//{
	//	public static void ClearAddRange<T>(this ObservableCollection<T> source, IEnumerable<T> range)
	//	{
	//		//var dispatcher = Application.Current.Dispatcher;
	//		//dispatcher.BeginInvoke(new Action(() =>			{
	//		source.Clear();
	//		range.ToList().ForEach(source.Add);
	//		Trace.WriteLine(range);
	//		//}));
	//	}
	//}

}