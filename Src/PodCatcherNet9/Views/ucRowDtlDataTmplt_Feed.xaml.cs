using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PodCatcherNet9.Views
{
    public partial class ucRowDtlDataTmplt_Feed : UserControl
  {
    public ucRowDtlDataTmplt_Feed()    {      InitializeComponent();    }
    void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) { Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)); e.Handled = true; }
  }
}
