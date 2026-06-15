using System.Windows;
using System.Windows.Controls;

namespace PodCatcherNet9.Views
{
    public partial class ucSimple : UserControl
	{
		public ucSimple()
		{
			InitializeComponent();
		}

		//public long Txt { get { return (long)GetValue(TxtProperty); } set { SetValue(TxtProperty, value); } }  public static readonly DependencyProperty TxtProperty = DependencyProperty.Register("Txt", typeof(long), typeof(ucSimple), new PropertyMetadata(10L));

		public string Txt { get { return (string)GetValue(TxtProperty); } set { SetValue(TxtProperty, value); } }  public static readonly DependencyProperty TxtProperty = DependencyProperty.Register("Txt", typeof(string), typeof(ucSimple), new PropertyMetadata("C#"));
		

	}
}
