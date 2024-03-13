using Db.PodcastMgt.PowerTools.Models;
using PodCatcherNet8;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PodCatcher.Views
{
    public partial class xDataContextRecon : Window
    {
        PodcastMgtContext db = new PodcastMgtContext(null);

        public xDataContextRecon() { InitializeComponent(); MouseLeftButtonDown += (s, e) => DragMove(); KeyDown += (s, e) => { switch (e.Key) { case Key.Escape: Close(); App.Current.Shutdown(); break; } }; }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            db.Feeds.Load();
            ((System.Windows.Data.CollectionViewSource)(this.FindResource("feedViewSource"))).Source = db.Feeds.Local;
        }

        void onSetNoteText(object sender, RoutedEventArgs e) { db.Feeds.ToList().ForEach(r => r.Note = "Test"); }

        void on1(object sender, RoutedEventArgs e)        {        }
        void on2(object sender, RoutedEventArgs e)        {        }
        void on3(object sender, RoutedEventArgs e)        {        }
    }
}
