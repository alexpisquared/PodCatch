using AsLink;
using System.Windows;
using PodCatcher.ViewModels;

namespace PodCatcher.Views
{
    public partial class vwMain__GoodOldRetired___ : Window
  {
    public vwMain__GoodOldRetired___()
    {
      InitializeComponent();

      //'tbFilter.Focus();
      //PreviewKeyDown += (s, e) => { if (dg1.SelectedItem != null) dg1.ScrollIntoView(dg1.SelectedItem); };
      MouseLeftButtonDown += (s, e) => DragMove();

      //Resources.MergedDictionaries[0].MergedDictionaries.Clear();			Resources.MergedDictionaries[0].MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/City.xaml", UriKind.RelativeOrAbsolute) });
      //'Application.Current.Resources.Source = new Uri("/Themes/City.xaml", UriKind.RelativeOrAbsolute);
      //?AllowsTransparency = true;

      AppSettings.RestoreSizePosition(this, Settings.Default.AppSettings);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      base.OnClosing(e);
      Settings.Default.AppSettings = AppSettings.SaveSizePosition(this, Settings.Default.AppSettings);
      Settings.Default.Save();
    }

    void Button_Click(object sender, RoutedEventArgs e) { BindableBaseViewModel.ShowMvvm(new AsyncFineTuningVM(false), new vwAsyncFineTuning()); }
  }
}
