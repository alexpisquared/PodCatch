using System;
using System.Windows;
using System.Windows.Controls;

namespace PodCatcher
{
    /// <summary>
    /// Interaction logic for SpeedProj.xaml
    /// </summary>
    public partial class SpeedProj : UserControl
	{
		public SpeedProj()
		{
			InitializeComponent();
		}

		public Nullable<System.DateTime> Time0 { get { return (Nullable<System.DateTime>)GetValue(Time0Property); } set { SetValue(Time0Property, value); } }  public static readonly DependencyProperty Time0Property = DependencyProperty.Register("Time0", typeof(Nullable<System.DateTime>), typeof(SpeedProj), new PropertyMetadata(null));
		public long LenTotal { get { return (long)GetValue(LenTotalProperty); } set { SetValue(LenTotalProperty, value); } }  public static readonly DependencyProperty LenTotalProperty = DependencyProperty.Register("LenTotal", typeof(long), typeof(SpeedProj), new PropertyMetadata(0L));
		public long? LenCurrent { get { return (long?)GetValue(LenCurrentProperty); } set { SetValue(LenCurrentProperty, value); } }  public static readonly DependencyProperty LenCurrentProperty = DependencyProperty.Register("LenCurrent", typeof(long?), typeof(SpeedProj), new PropertyMetadata(0L, progressed));

		private static void progressed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var o = d as SpeedProj;
			if (o.LenCurrent == null) return;

			var now = DateTime.Now;
			if (o.Time0.HasValue && o.Time0 != DateTime.MinValue && now != o.Time0 && (o.LenTotal - o.LenCurrent.Value) > 0)
			{
				var bpm = (o.LenCurrent.Value) / (now - o.Time0.Value).TotalMinutes;
				o.SpeedMbMin = $"{bpm / 1048576,5:N1}";

				var minutesLeft = (bpm != 0) ? (o.LenTotal - o.LenCurrent.Value) / bpm : -1;
				if (minutesLeft < 10000)
				{
					o.CurrentETA = $"{now.AddMinutes(minutesLeft):HH:mm} ";
					o.MinutsLeft = $"{minutesLeft:N1} ";
				}
				else
					o.CurrentETA = "max";
			}
			else
			{
				o.SpeedMbMin = "";
				o.MinutsLeft = "";
				o.CurrentETA = "?";
			}
		}

		public string SpeedMbMin { get { return (string)GetValue(SpeedMbMinProperty); } set { SetValue(SpeedMbMinProperty, value); } }  public static readonly DependencyProperty SpeedMbMinProperty = DependencyProperty.Register("SpeedMbMin", typeof(string), typeof(SpeedProj), new PropertyMetadata());
		public string MinutsLeft { get { return (string)GetValue(MinutsLeftProperty); } set { SetValue(MinutsLeftProperty, value); } }  public static readonly DependencyProperty MinutsLeftProperty = DependencyProperty.Register("MinutsLeft", typeof(string), typeof(SpeedProj), new PropertyMetadata());
		public string CurrentETA { get { return (string)GetValue(CurrentETAProperty); } set { SetValue(CurrentETAProperty, value); } }  public static readonly DependencyProperty CurrentETAProperty = DependencyProperty.Register("CurrentETA", typeof(string), typeof(SpeedProj), new PropertyMetadata());
	}
}
