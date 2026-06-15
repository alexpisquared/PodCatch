using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

//xmlns:mvvm="clr-namespace:SBNET.MVVM;assembly=SBNET.MVVM"
//<TextBlock TextAlignment="Center" Text="{Binding Path=Unlading, Converter={mvvm:Equals EqualsText='Y', NotEqualsText='N'}, ConverterParameter=1}" />

namespace PodCatcherNet9.Helpers
{
  public class IsTodayClr : MarkupExtension, IValueConverter
  {
    public bool IsInverted { get => _IsInverted; set => _IsInverted = value; }
    bool _IsInverted = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is DateTime && (DateTime)value >= DateTime.Today ? Brushes.LightGreen : Brushes.LightPink;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public IsTodayClr() { }
  }

  public class LenToVis : MarkupExtension, IValueConverter
  {
    public bool IsInverted { get => _IsInverted; set => _IsInverted = value; }
    bool _IsInverted = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string && value != null && ((string)value).Length > 0)
        return IsInverted ? Visibility.Collapsed : Visibility.Visible;
      else
        return !IsInverted ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public LenToVis() { }
  }

  public class Divider : MarkupExtension, IValueConverter
  {
    public int Divisor { get => divisor; set => divisor = value; }
    int divisor = 1000000;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

      if (value == null) return null;

      if (Divisor == 0) return value;

      if (value is int) return (float)(int)value / divisor;
      if (value is long) return (float)(long)value / divisor;
      if (value is float) return (float)value / divisor;
      if (value is double) return (double)value / divisor;

      return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public Divider() { }
  }
  public class RssTextTrimmer : IValueConverter
  {
    // Clean up text fields from each SyndicationItem. 
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null) return null;

      var maxLength = 2000;
      var strLength = 0;
      var fixedString = "";

      // Remove HTML tags and newline characters from the text, and decodes HTML encoded characters. 
      // This is a basic method. Additional code would be needed to more thoroughly  
      // remove certain elements, such as embedded Javascript. 

      // Remove HTML tags. 
      fixedString = Regex.Replace(value.ToString(), "<[^>]+>", string.Empty);

      // Remove newline characters
      fixedString = fixedString.Replace("\r", "").Replace("\n", "");

      // Remove encoded HTML characters 			fixedString = System.Net.HttpUtility.HtmlDecode(fixedString);

      strLength = fixedString.ToString().Length;

      // Some feed management tools include an image tag in the Description field of an RSS feed, 
      // so even if the Description field (and thus, the Summary property) is not populated, it could still contain HTML. 
      // Due to this, after we strip tags from the string, we should return null if there is nothing left in the resulting string. 
      if (strLength == 0)
        return null;

      // Truncate the text if it is too long. 
      else if (strLength >= maxLength)
      {
        fixedString = fixedString.Substring(0, maxLength);

        // Unless we take the next step, the string truncation could occur in the middle of a word.
        // Using LastIndexOf we can find the last space character in the string and truncate there. 
        fixedString = fixedString.Substring(0, fixedString.LastIndexOf(" "));
      }

      fixedString += "...";

      return fixedString;
    }

    // This code sample does not use TwoWay binding and thus, we do not need to flesh out ConvertBack.  
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
  }
  public class StatusBrush : MarkupExtension, IValueConverter
  {
    public bool IsInverted { get => _IsInverted; set => _IsInverted = value; }
    bool _IsInverted = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string && value != null && targetType == typeof(Brush))) return Brushes.Yellow;

      var v = (string)value;
      switch (v)
      {
        case "A": return Brushes.LightSkyBlue;  //	All Done
        case "C": return Brushes.Khaki;         //	Cut into 1 min chunks is finished	NULL
        case "F": return Brushes.PaleVioletRed; //	Failed 
        case "H": return Brushes.RosyBrown;     //	HasBeenDownloaded	NULL
        case "I": return Brushes.Yellow;        //	IsBeingDownloaded	NULL
        case "M": return Brushes.Pink;          //	Marked for Download	NULL
        case "N": return Brushes.WhiteSmoke;    //	New	NULL
        case "...": return Brushes.LightGray;   //	Feeds started processing
        default: break;
      }

      if (v.StartsWith("0 /")) return Brushes.Gray;
      if (v.Contains(" / ")) return Brushes.Black;

      var ttl = 0;
      foreach (var item in (string)value)
        ttl += item;

      var r = (byte)(ttl % 256);
      var g = (byte)(128 * (ttl % 5));
      var b = (byte)(255 - r);
      var clr = Color.FromRgb(r, g, b);

      return new SolidColorBrush(clr);
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public StatusBrush() { }
  }

  public class SelectedConverter : MarkupExtension, IValueConverter
  {
    bool _IsInverted = false; public bool IsInverted { get => _IsInverted; set => _IsInverted = value; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool && (bool)value && targetType == typeof(Brush))
        return
            //new LinearGradientBrush(Colors.Green, Colors.LightGreen, -90);
            //new LinearGradientBrush(new GradientStopCollection() { new GradientStop(Color.FromRgb(200, 255, 200), 0.0), new GradientStop(Color.FromRgb(155, 255, 155), 1.0) }, 90);//
            //new LinearGradientBrush(new GradientStopCollection() { new GradientStop(Color.FromRgb(0xdB, 0xf0, 0xFf), 0.0), new GradientStop(Color.FromRgb(0xC1, 0xE0, 0xF0), 1.0) }, 90);// 0xCB, 0xE8, 0xF6
            new LinearGradientBrush(new GradientStopCollection() { new GradientStop(Color.FromRgb(0xdB, 0xFf, 0xd0), 0.0), new GradientStop(Color.FromRgb(0xa0, 0xF8, 0xa0), 1.0) }, 90);// 0xCB, 0xE8, 0xF6


      return null;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public SelectedConverter() { }
  }
  public class UniConverter : MarkupExtension, IValueConverter
  {
    bool _IsInverted = false; public bool IsInverted { get => _IsInverted; set => _IsInverted = value; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string)
        switch ((string)value)
        {
          case "offline":
            if (targetType == typeof(Brush)) return Brushes.White;
            else if (targetType == typeof(bool)) return false;
            else if (targetType == typeof(Visibility)) return Visibility.Collapsed;
            else return null;
          case "online":
            if (targetType == typeof(Brush)) return new BrushConverter().ConvertFromString("#00ff00");//new LinearGradientBrush(Colors.Green, Colors.LightGreen, 0);//
            else if (targetType == typeof(bool)) return true;
            else if (targetType == typeof(Visibility)) return Visibility.Visible;
            else return null;
          case "DND":
            if (targetType == typeof(Brush)) return new LinearGradientBrush(new GradientStopCollection() { new GradientStop(Colors.Red, 0.0), new GradientStop(Colors.Red, 0.44), new GradientStop(Colors.White, 0.45), new GradientStop(Colors.White, 0.55), new GradientStop(Colors.Red, 0.56), new GradientStop(Colors.Red, 1.0) }, 90);//
            else if (targetType == typeof(bool)) return true;
            else if (targetType == typeof(Visibility)) return Visibility.Visible;
            else return null;
          case "page-me":
            if (targetType == typeof(Brush)) return new BrushConverter().ConvertFromString("#ff8000");//new LinearGradientBrush(Colors.Green, Colors.LightGreen, 0);//
            else if (targetType == typeof(bool)) return true;
            else if (targetType == typeof(Visibility)) return Visibility.Visible;
            else return null;
          case "sign-in":
          case "sign-out":
          case "change-state":
          case "state":
          case "disconnecting":
          case "shutting-down":
          case "acquiring-network":
          case "connecting":
          case "synchronizing":
          case "no-network":
          case "no-service":
          case "quit":
          case "terminate": return Brushes.Black;
          default:
            if (targetType == typeof(Visibility)) return string.IsNullOrEmpty((string)value) && IsInverted ? Visibility.Visible : Visibility.Collapsed;
            else if (targetType == typeof(Brush)) return Brushes.Gray;
            else return Brushes.Gray;
        }
      else if (value is bool)
        if (targetType == typeof(bool)) return (bool)value ? true : false;
        else if (targetType == typeof(Brush))
          return (bool)value ? new LinearGradientBrush(Colors.Green, Colors.LightGreen, 0) : new LinearGradientBrush(Colors.DarkRed, Colors.Red, 0);//new BrushConverter().ConvertFromString("#00ff00");
        else if (targetType == typeof(Visibility))
          return (bool)value ? _IsInverted ? Visibility.Collapsed : Visibility.Visible : _IsInverted ? Visibility.Visible : Visibility.Collapsed;
        else if (targetType == typeof(FontWeight))
          return (bool)value ? _IsInverted ? FontWeights.Normal : FontWeights.Bold : _IsInverted ? FontWeights.Bold : FontWeights.Normal;
        else return null;

      if (targetType == typeof(Visibility) && value == null)
        return
          //_IsInverted ? Visibility.Visible : 
          Visibility.Collapsed;

      return new LinearGradientBrush(Colors.Gray, Colors.DarkGray, 45);//		return new BrushConverter().ConvertFromString("#ff0000");
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public UniConverter() { }
  }
}
