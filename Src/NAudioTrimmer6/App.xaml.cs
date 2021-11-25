using System.Windows;
using NAudioTrimmer6.Logic;

namespace NAudioTrimmer6
{
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      new MainWindow(new NAudioHelper(), new BitmapHelper()).Show();
    }
  }
}
