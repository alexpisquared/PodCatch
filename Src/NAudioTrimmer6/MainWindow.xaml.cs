using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//using System.Windows.Shapes;
using System.Windows.Threading;
using NAudioTrimmer6.Logic;

namespace NAudioTrimmer6
{
  public partial class MainWindow : Window
  {
    readonly INAudioHelper _nauHelper;
    readonly IBitmapHelper _bmpHelper;
    readonly DispatcherTimer _resettter, _progMover;
    bool _isPlaying;
    string SettingsDefaultLastFile;

    public MainWindow(INAudioHelper naHelper, BitmapHelper waveImage)
    {
      InitializeComponent();

      _nauHelper = naHelper;
      _bmpHelper = waveImage;

      dragPnl.MouseLeftButtonDown += (s, e) => DragMove();
      me1.MediaOpened += mediaOpened;
      me1.MediaFailed += mediaFailed;
      me1.MediaEnded += mediaEnded;
      Loaded += onLoaded;
      KeyDown += (s, e) =>
      {
        TimeSpan step =
                  e.KeyboardDevice.Modifiers == ModifierKeys.Alt ? TimeSpan.FromMilliseconds(100000) :
                  e.KeyboardDevice.Modifiers == ModifierKeys.None ? TimeSpan.FromMilliseconds(1000) :
                  e.KeyboardDevice.Modifiers == ModifierKeys.Shift ? TimeSpan.FromMilliseconds(100) :
                  e.KeyboardDevice.Modifiers == ModifierKeys.Control ? TimeSpan.FromMilliseconds(10000) :
                  e.KeyboardDevice.Modifiers == ModifierKeys.Windows ? TimeSpan.FromMilliseconds(1000) : TimeSpan.FromMilliseconds(1000);
        switch (e.Key)
        {
          case Key.Space: onTglPlay(s, e); break;
          case Key.Escape: Close(); Application.Current.Shutdown(); break;
          case Key.Left: me1.Position = me1.Position.Add(-step); break;
          case Key.Right: me1.Position = me1.Position.Add(step); break;
        }
      };

      _progMover = new DispatcherTimer(TimeSpan.FromSeconds(.02), DispatcherPriority.Normal, new EventHandler((s, e) => onPbMover()), Dispatcher.CurrentDispatcher);
      _resettter = new DispatcherTimer(TimeSpan.FromSeconds(2), DispatcherPriority.Background, new EventHandler((s, e) => onBackToPositionA(s, null)), Dispatcher.CurrentDispatcher); //tu: one-line timer
      _resettter.Stop();
    }
    void slS_PreviewKeyDown(object s, KeyEventArgs e)    {    }      //            e.Handled = true;
    async void onLoaded(object s, RoutedEventArgs e)
    {
      //if (string.IsNullOrEmpty(Settings.Default.LastFile)) return;

      //await playNewFile(Settings.Default.LastFile);
    }
    async void mediaOpened(object s, RoutedEventArgs e)
    {
      hlPath.NavigateUri = new Uri(tbPath.Text = $"{System.IO.Path.GetDirectoryName(me1.Source.LocalPath)}\\");
      tbFile.Text = $"{System.IO.Path.GetFileName(me1.Source.LocalPath)}";

      TimeSpan ts = (me1.NaturalDuration.HasTimeSpan ? me1.NaturalDuration.TimeSpan : TimeSpan.Zero);
      tbPntA.Text = $"{ts:h\\:mm\\:ss\\.ff}";

      slB.Value = slB.Maximum = slA.Maximum = pb1.Maximum = ts.TotalSeconds;
      _progMover.Start();
      await Task.Yield();
    }
    async void mediaFailed(object s, ExceptionRoutedEventArgs e) => await Task.Yield();
    async void mediaEnded(object s, RoutedEventArgs e) => await Task.Delay(25); /*_progMover.Stop();*/
    async void onDrop(object s, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

      var files = e.Data.GetData(DataFormats.FileDrop) as string[];
      if (files.Length < 1) return;

      var csv = string.Join("|", files);      //if (ex.KeyStates == DragDropKeyStates.ControlKey)			//	m.LoadNewMedia(csv);//TODO: Add to the curent list			//else			//	m.LoadNewMedia(csv);

      SettingsDefaultLastFile = files.First();
      //Settings.Default.Save();

      await playNewFile(SettingsDefaultLastFile);
    }
    async void slA_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e) { tbPntA.Text = $"{(me1.Position = TimeSpan.FromSeconds(slA.Value)):h\\:mm\\:ss\\.ff}"; if (slB.Value < slA.Value) slB.Value = slA.Value + 60; await Task.Yield(); }
    async void slB_ValueChanged(object s, RoutedPropertyChangedEventArgs<double> e) { tbPntB.Text = $"{(TimeSpan.FromSeconds(slB.Value)):h\\:mm\\:ss\\.ff}"; if (slA.Value > slB.Value) slA.Value = slB.Value - 60; await Task.Yield(); }
    async void onBackToPositionA(object s, RoutedEventArgs e) { me1.Position = TimeSpan.FromSeconds(slA.Value); me1.Play(); _isPlaying = true; await Task.Yield(); }
    async void onMediaPosnToSliderA(object s, RoutedEventArgs e) { slA.Value = me1.Position.TotalSeconds; await Task.Yield(); }
    async void onMediaPosnToSliderB(object s, RoutedEventArgs e) { slB.Value = me1.Position.TotalSeconds; await Task.Yield(); }
    async void onTrimBoth(object s, RoutedEventArgs e) => await trim(_nauHelper.TrimMp3Both);// async void onTrimLeft(object s, RoutedEventArgs e) => await trim(_naHelper.TrimMp3Left); async void onTrimRght(object s, RoutedEventArgs e) => await trim(_naHelper.TrimMp3Rght);
    async void onTglPlay(object s, RoutedEventArgs e) { if (_isPlaying) me1.Pause(); else me1.Play(); _isPlaying = !_isPlaying; await Task.Yield(); }
    async void pb1_MouseUp_L_A(object s, MouseButtonEventArgs e) { slA.Value = me1.NaturalDuration.TimeSpan.TotalSeconds * ((MouseDevice)e.Device).GetPosition(root).X / ((ProgressBar)s).ActualWidth; await Task.Yield(); }
    async void pb1_MouseUp_R_B(object s, MouseButtonEventArgs e) { slB.Value = me1.NaturalDuration.TimeSpan.TotalSeconds * ((MouseDevice)e.Device).GetPosition(root).X / ((ProgressBar)s).ActualWidth; await Task.Yield(); }
    async void pb1_MousMov(object s, /**/  MouseEventArgs e) => await Task.Yield();             //slB.Value = me1.NaturalDuration.TimeSpan.TotalSeconds * ((MouseDevice)e.Device).GetPosition(root).X / ((ProgressBar)s).ActualWidth;
    async void onPbMover()
    {
      pb1.Value = me1.Position.TotalSeconds; tbPPos.Text = $"{me1.Position:h\\:mm\\:ss\\.fff}";
      if (me1.Position.TotalSeconds >= slB.Value && slB.Value > 0)
      {
        me1.Pause();
        _isPlaying = false;
      }

      if (me1.NaturalDuration.HasTimeSpan)
        sv1.ScrollToHorizontalOffset(str1.ActualWidth * me1.Position.TotalSeconds / me1.NaturalDuration.TimeSpan.TotalSeconds);

      await Task.Yield();
    }
    async void onTglAutoResetter(object s, RoutedEventArgs e) { _resettter.IsEnabled = ((CheckBox)s).IsChecked == true; await Task.Yield(); }
    async void onRequestNavigate(object s, System.Windows.Navigation.RequestNavigateEventArgs e) { Process.Start(new ProcessStartInfo(Path.Combine(e.Uri.LocalPath, ""))); e.Handled = true; await Task.Yield(); }

    async Task playNewFile(string file)
    {
      if (File.Exists(file))
      {
        me1.Source = new Uri(file); me1.Play(); _isPlaying = true; await loadShowWaveForm();
      }
      else
      {
        tbPPos.Text = $"File {file} does not exist. Try another one.";
      }
    }
    async Task trim(Action<string, string, TimeSpan, TimeSpan> func)
    {
      cntrlPnl.Visibility = Visibility.Hidden;
      await Task.Delay(100);
      me1.Pause();
      var trg = Path.Combine(Path.GetDirectoryName(me1.Source.LocalPath), "Trimmed", Path.GetFileName(me1.Source.LocalPath)); //tu: Path.ChangeExtension(me1.Source.LocalPath, ".trimmed.mp3")

      func.Invoke(me1.Source.LocalPath, trg, TimeSpan.FromSeconds(slA.Value), TimeSpan.FromSeconds(slB.Value));

      me1.Play(); _isPlaying = true;
      await Task.Delay(100);
      cntrlPnl.Visibility = Visibility.Visible;
    }
    async Task loadShowWaveForm()
    {
      //((Button)s).Visibility = Visibility.Hidden;
      tbPntB.Text = $"Loading waveform...";

      im1.Source = null;
      await Task.Run(() => _nauHelper.LoadWaveFormToBitmap(SettingsDefaultLastFile)).ContinueWith(_ =>
      {
        im1.Source = _bmpHelper.B2bs(_.Result.Item1);
        tbPntB.Text = $"{_.Result.Item2:m\\:ss\\.ff}";
        //((Button)s).Visibility = Visibility.Visible;
      }, TaskScheduler.FromCurrentSynchronizationContext());
    }
  }
}
