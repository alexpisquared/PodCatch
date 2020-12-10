using AAV.Sys.Ext;
using AsLink;
using PodcastClientTpl;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PodCatcher.Views
{
  public partial class vwMediaPlayer : Window
  {

    public vwMediaPlayer()
    {
      InitializeComponent();
      MouseLeftButtonDown += (s, e) => DragMove();

      me1.LoadedBehavior = MediaState.Play;
      me1.Loaded += Me1_Loaded;
      Title = "";
    }

    private void Me1_Loaded(object sender, RoutedEventArgs e)
    {
      if (me1.NaturalDuration.HasTimeSpan)
      {
        Title += $"L {me1.NaturalDuration.TimeSpan.TotalMinutes:N1}  ";
        Debug.WriteLine(Title);
      }
    }

    public async Task<double> GetMediaDurationInMinUsingMediaElement(string mediaFile)
    {
      tb1.Text = Title = $"{System.IO.Path.GetFileName(mediaFile)} ";

      var ttlMin = await getDurnUsingMediaElnt(mediaFile);

      for (int j = 20; j > 0 && ttlMin == ConstHelper.Unknown4004Duration; j--)
      {
        await Task.Delay(3000);
        await Application.Current.Dispatcher.BeginInvoke(new Action(async () => { ttlMin = await getDurnUsingMediaElnt(mediaFile); }));
        tb1.Text = Title += $"{j} ";
      }

      if (ttlMin == ConstHelper.Unknown4004Duration || ttlMin > 300)
      {
        ttlMin = NAudioHelper.GetDuration(mediaFile).TotalMinutes;
      }

      tb1.Text = Title += $" => {ttlMin:N1}min.";

      tb1.Foreground = new SolidColorBrush((ttlMin == ConstHelper.Unknown4004Duration || ttlMin > 300) ? Colors.Red : Colors.Green);

      return ttlMin;
    }
    async Task<double> getDurnUsingMediaElnt(string mediaFile)
    {
      try
      {
        me1.Source = new Uri(mediaFile);

        for (int i = 0; i < 33; i++)
        {
          if (me1.NaturalDuration.HasTimeSpan)
          {
            Title += $"i{i} ";
            Debug.WriteLine(Title);
            return me1.NaturalDuration.TimeSpan.TotalMinutes;
          }

          Show();

          await Task.Delay(1000);
        }
      }
      catch (Exception ex) { ex.Log(); }
      finally
      {
        await Task.Delay(200);
        me1.LoadedBehavior = MediaState.Manual;
        me1.Stop();
#if !DEBUG
        //Close();
#endif
      }

      return ConstHelper.Unknown4004Duration;
    }
  }
}
