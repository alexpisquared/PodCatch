using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using PodCatcher.ViewModels;
using PodCatcher.Views;
using WpfUserControlLib.Helpers;

namespace PodCatcherNet8;
public partial class App : Application
{
  public static DateTime AppStartAt = DateTime.Now;

  protected override void OnStartup(StartupEventArgs e)
  {
    Application.Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox).SelectAll(); })); //tu: TextBox
    ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue)); //tu: ToolTip ShowDuration !!!

    //DevOpStartup.SetupTracingOptions("PodCatcher");            //ShutdownMode = ShutdownMode.OnExplicitShutdown;

    base.OnStartup(e);      //dbIni: //DBInitializer.DropCreateDB();				//test: var _db = new MediaQADB();				_db.MediaInfos.Load();				foreach (var mi in _db.MediaInfos.Local) Console.WriteLine(mi); 

    var vm = new AsyncFineTuningVM(true);
    var vw = new vwAsyncFineTuning();
    vw.DataContext = vm;
    vw.ShowDialog();    

    Thread.Sleep(500);
    Application.Current.Shutdown();
  }
}

