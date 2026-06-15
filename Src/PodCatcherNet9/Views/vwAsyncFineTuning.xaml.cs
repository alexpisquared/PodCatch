using System.Diagnostics;
using System.IO;
using System.Windows;
using PodcastClientTpl;
using PodCatcherNet9.Helpers;

namespace PodCatcherNet9.Views;

public partial class vwAsyncFineTuning : AAV.WPF.Base.WindowBase
{
  public vwAsyncFineTuning() => InitializeComponent();
  string FilenameONLY => $"{GetType().Name}.ZV.xml";

  void Hyperlink_RequestNavigate_1(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) { _ = Process.Start(new ProcessStartInfo(Path.Combine(e.Uri.LocalPath, ConstHelper._Mirr_))); e.Handled = true; }
  void Hyperlink_RequestNavigate_2(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) { _ = MessageBox.Show(PostDnldHelper.CopyToMp3Player(), "PostDnldHelper.CopyToMp3Player()"); e.Handled = true; }
  void Button_Click(object sender, RoutedEventArgs e) => DataContext = null;
  void OnClose(object sender, RoutedEventArgs e) => Close();
}