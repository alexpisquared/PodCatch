using System.Windows;
using Db.PodcastMgt.DbModel;

namespace PodCatcherNet9.Views
{
    public partial class vwAddFeed : Window
	{
		public vwAddFeed()
		{
			InitializeComponent();

			Loaded += VwAddFeed_Loaded;

			DataContext = this;
			e1.Focus();
		}

		void VwAddFeed_Loaded(object sender, RoutedEventArgs e) { inferFromClipboard(); }

		void inferFromClipboard()
		{
			var cb = Clipboard.GetText();
			if (!string.IsNullOrEmpty(cb))
			{
				var ss = cb.Split('\n');
				if (ss.Length == 1) { Feed.Url = cb; Feed.Name = "*NEW* " + cb.Replace("https://", "").Replace("http://", "").Replace("www", "").Replace(".", " ").Replace("/", " ").Replace("?", " ").Trim(); }
				if (ss.Length == 2) { Feed.Name = ss[0]; Feed.Note = ss[1]; }
			}
		}

		public static readonly DependencyProperty FeedProperty = DependencyProperty.Register("Feed", typeof(Feed), typeof(vwAddFeed), new PropertyMetadata(null)); public Feed Feed { get { return (Feed)GetValue(FeedProperty); } set { SetValue(FeedProperty, value); } }

		//public static readonly DependencyProperty UrlProperty = DependencyProperty.Register("Url", typeof(string), typeof(vwAddFeed), new PropertyMetadata("")); public string RSUrl { get { return (string)GetValue(UrlProperty); } set { SetValue(UrlProperty, value); } }
		//public static readonly DependencyProperty NamProperty = DependencyProperty.Register("Name", typeof(string), typeof(vwAddFeed), new PropertyMetadata("")); public string PName { get { return (string)GetValue(NamProperty); } set { SetValue(NamProperty, value); } }
		//public static readonly DependencyProperty NoteProperty = DependencyProperty.Register("ErrLog", typeof(string), typeof(vwAddFeed), new PropertyMetadata("")); public string PNote { get { return (string)GetValue(NoteProperty); } set { SetValue(NoteProperty, value); } }
		//public static readonly DependencyProperty SinceProperty = DependencyProperty.Register("Since", typeof(DateTime), typeof(vwAddFeed), new PropertyMetadata(DateTime.Today.AddDays(-1))); public DateTime Since { get { return (DateTime)GetValue(SinceProperty); } set { SetValue(SinceProperty, value); } }
		//public static readonly DependencyProperty IsActivProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(vwAddFeed), new PropertyMetadata(true)); public bool Active { get { return (bool)GetValue(IsActivProperty); } set { SetValue(IsActivProperty, value); } }

		void OnOK(object sender, RoutedEventArgs e) { DialogResult = true; Close(); }
		void OnCancel(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
	}
}
