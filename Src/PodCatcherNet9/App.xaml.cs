using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AAV.WPF.Helpers;
using AsLink;
using PodCatcherNet9.ViewModels;
using PodCatcherNet9.Views;
using AAV.WPF.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace PodCatcherNet9;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
  public static DateTime AppStartAt = DateTime.Now;

  protected override void OnStartup(StartupEventArgs e)
  {
    Application.Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox).SelectAll(); })); //tu: TextBox
    ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue)); //tu: ToolTip ShowDuration !!!

    //DevOpStartup.SetupTracingOptions("PodCatcherNet9");            //ShutdownMode = ShutdownMode.OnExplicitShutdown;

    base.OnStartup(e);      //dbIni: //DBInitializer.DropCreateDB();				//test: var _db = new MediaQADB();				_db.MediaInfos.Load();				foreach (var mi in _db.MediaInfos.Local) Console.WriteLine(mi); 

#if DEBUG__
            //var vw = new xDataContextRecon();
            //var dc  = new AsyncFineTuningVM(true);
            //dc.Load();
            //BindableBaseViewModel.ShowModalMvvm(dc, vw);
            //vw.DataContext = dc;
            //vw.ShowDialog();


            //await new AsyncFineTuningVM(false).asy4AnonsGenr();
            //NAudioHelper.Test();
            //var rv = Helpers.PostDnldHelper.CopyToMp3Player();
            //AdvertCutter.WavDevDbgPoc();
            PodcastConditioning.AdvertCutter.CreateSummaryAnons(123, @"D:\Users\alex\Videos\0Pod\_Player\_Player");
            //Process.Start(new ProcessStartInfo("cmd", " /k robocopy"));      Application.Current.Shutdown();      return;
            ////new PodCatcherNet9.RAD.FeedDbGrid().ShowDialog();			
            ////BindableBaseViewModel.ShowMvvm(new AsyncFineTuningVM(false), new vwAsyncFineTuning());
            //var vm = new PodCatcherViewModel(false);
            //var vw = new vwMain__Old();
            //((PodCatcherViewModel)vm).Pnl1 = (((vwMain__Old)vw).pnl1);
            //var rv = BindableBaseViewModel.ShowModalMvvm(vm, vw);
#else
    MVVM.Common.BindableBaseViewModel.ShowModalMvvm(new AsyncFineTuningVM(true), new vwAsyncFineTuning());
#endif

    Thread.Sleep(500);
    Application.Current.Shutdown();
  }
}

