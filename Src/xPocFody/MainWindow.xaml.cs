using Db.PodcastMgt.DbModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace xPocFody
{
    public partial class MainWindow : Window
	{
        //DbModel3.Model1 db = new DbModel3.Model1();
        A0DbContext db = A0DbContext.Create();

		public MainWindow() { InitializeComponent(); MouseLeftButtonDown += (s, e) => DragMove(); KeyDown += (s, e) => { switch (e.Key) { case Key.Escape: Close(); App.Current.Shutdown(); break; } }; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            db.Machines.Load();
            db.Feeds.Load();
            ((CollectionViewSource)(this.FindResource("machineViewSource"))).Source = db.Machines.Local;
            System.Windows.Data.CollectionViewSource feedViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("feedViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // feedViewSource.Source = [generic data source]
        }

        void onSetNoteText(object sender, RoutedEventArgs e) { db.Feeds.ToList().ForEach(r => r.Note = "Test"); }
	}
}
