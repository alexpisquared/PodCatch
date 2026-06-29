using StandardLib.Helpers;
using PodCatcherNet9.ViewModels;
using PodCatcherNet9.Views;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
namespace PodCatcherNet9;
public partial class App : Application
{
  public static DateTime AppStartAt = DateTime.Now;
  private static Timer? _scheduledAutoShutdownTimer;

  protected override async void OnStartup(StartupEventArgs e)
  {
    //Application.Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox).SelectAll(); })); //tu: TextBox
    ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue)); //tu: ToolTip ShowDuration !!!

    //DevOpStartup.SetupTracingOptions("PodCatcherNet9");            //ShutdownMode = ShutdownMode.OnExplicitShutdown;

    base.OnStartup(e);      //dbIni: //DBInitializer.DropCreateDB();				//test: var _db = new MediaQADB();				_db.MediaInfos.Load();				foreach (var mi in _db.MediaInfos.Local) Console.WriteLine(mi); 

    if (Array.Exists(e.Args, arg => arg.Contains("Schedule", StringComparison.OrdinalIgnoreCase)))
    {
      _scheduledAutoShutdownTimer = new Timer(_ =>
      {
        _scheduledAutoShutdownTimer?.Dispose();
        _scheduledAutoShutdownTimer = null;
        Current?.Dispatcher.Invoke(() => Current?.Shutdown());
      }, null, TimeSpan.FromMinutes(8), Timeout.InfiniteTimeSpan);
    }

#if DEBUG__
            //var vw = new xDataContextRecon();
            //var dc  = new AsyncFineTuningVM(true);
            //dc.Load();
            //BindableBaseViewModel.ShowModalMvvm(dc, vw);
            //vw.DataContext = dc;
            //vw.ShowDialog();


            //await new AsyncFineTuningVM(false).asy4AnnonsGenr();
            //NAudioHelper.Test();
            //var rv = Helpers.PostDnldHelper.CopyToMp3Player();
            //AdvertCutter.WavDevDbgPoc();
            PodcastConditioning.AdvertCutter.CreateSummaryAnnons(123, @"D:\Users\alex\Videos\0Pod\_Player\_Player");
            //Process.Start(new ProcessStartInfo("cmd", " /k robocopy"));      Application.Current.Shutdown();      return;
            ////new PodCatcherNet9.RAD.FeedDbGrid().ShowDialog();			
            ////BindableBaseViewModel.ShowMvvm(new AsyncFineTuningVM(false), new vwAsyncFineTuning());
            //var vm = new PodCatcherViewModel(false);
            //var vw = new vwMain__Old();
            //((PodCatcherViewModel)vm).Pnl1 = (((vwMain__Old)vw).pnl1);
            //var rv = BindableBaseViewModel.ShowModalMvvm(vm, vw);
#else
    var vm = new AsyncFineTuningVM(true);
    MVVM.Common.BindableBaseViewModel.ShowMvvm(vm, new vwAsyncFineTuning());
    await vm.AutoExec__Async();
#endif
  }
}

