using Db.PodcastMgt.DbModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PodCatcherNet9.Views
{
    public partial class WindowJoin : Window
  {
    public WindowJoin()
    {
      InitializeComponent();
      KeyUp += (s, e) => { if (e.Key == Key.Escape) { Close(); App.Current.Shutdown(); } }; //tu:
    }


    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      A0DbContext db = A0DbContext.Create();
      //var lq = _db.Machines.Include(m => m.Feeds).ToList();
      //var blog1 = _db.Machines.Where(m => m.Id == " V AIO1").Include(m => m.Feeds).FirstOrDefault();			// Load one blogs and its related posts     - http://msdn.microsoft.com/en-ca/data/jj574232.aspx

      //_db.Machines.Where(m => m.Id == "v aio1").Load();//ok: ignore case SQL style.	+	((CollectionViewSource)(this.FindResource("machineViewSource"))).Source = _db.Machines.Local; //!!! ==> only case-sensitive matches


      db.Feeds.Where(r => /*string.Compare(r.HostMachineId, Environment.MachineName, true) == 0 &&*/ r.IsActive && !r.IsDeleted).Load(); ((CollectionViewSource)(this.FindResource("feedViewSource"))).Source = db.Feeds.Local;

    }
  }
}
