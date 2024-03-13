using StandardLib.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Db.PodcastMgt.PowerTools.Models;
using PodCatcherNet8;
using PodCatcherNet8.RAD;

namespace PodCatcher.RAD
{
  public partial class FeedDbGrid : Window
  {
    readonly PodcastMgtContext _dbx = new PodcastMgtContext(null);
    CancellationTokenSource _cts;
    public string InfoMsg { get; set; }// { return _InfoMsg; } set { this.Set(ref this._InfoMsg, value + "\r\n" + _InfoMsg); } }										string _InfoMsg = "";
    ObservableCollection<FeedNpc> _feedList;

    public FeedDbGrid() { InitializeComponent(); MouseLeftButtonDown += (s, e) => DragMove(); KeyDown += (s, e) => { switch (e.Key) { case Key.Escape: Close(); App.Current.Shutdown(); break; } }; }
    void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _feedList = loadFeedList(); ((CollectionViewSource)(FindResource("feedViewSource"))).Source = _feedList; Title =
$"{_feedList.Count} rows";
    }
    void start0Button_Click(object sender, RoutedEventArgs e) => startCheckingOfAllFeedsTasks();
    void cancelButton_Click(object sender, RoutedEventArgs e) { if (_cts != null) { _cts.Cancel(); } }

    async void startCheckingOfAllFeedsTasks()
    {
      t1.Text = "Starting...";
      _cts = new CancellationTokenSource();
      try
      {
        await startCheckingOfAllFeedsTasks(_cts.Token);
        t1.Text += "\r\nDownloads complete.";
      }
      catch (OperationCanceledException ex) { ex.Log(); t1.Text += "\r\nDownloads canceled.\r\n"; }
      catch (Exception ex) { ex.Log(); t1.Text += "\r\nDownloads failed.\r\n" + ex.ToString(); }
      _cts = null;
    }
    async Task startCheckingOfAllFeedsTasks(CancellationToken ct)
    {
      var client = new HttpClient();

      var downloadTasksQuery = from feed in _feedList select updateFeedWithDataFromWeb(feed, client, ct); // ***Create a query that, when executed, returns a collection of tasks.
      var downloadTasks = downloadTasksQuery.ToList();                                                            // ***Use ToList to execute the query and start the tasks. 

      while (downloadTasks.Count > 0)                                                                                             // ***Add a loop to process the tasks one at a time until none remain.
      {
        var firstFinishedTask = await Task.WhenAny(downloadTasks);                                                      // Identify the first task that completes.
        downloadTasks.Remove(firstFinishedTask);                                                                                  // ***Remove the selected task from the list so that you don't process it more than once.
        var feedCopyOrWhat = await firstFinishedTask;                                                                         // Await the completed task.
        t1.Text += $"\r\n RV:  {feedCopyOrWhat.Id,4}  '{feedCopyOrWhat.Note}' {""}";
      }
    }
    async Task<FeedNpc> updateFeedWithDataFromWeb(FeedNpc feed, HttpClient client, CancellationToken ct)
    {
      Debug.WriteLine("Going for:  {0,4} {1}", feed.Id, feed.Url);

      try
      {
        var response = await client.GetAsync(feed.Url, ct);     // GetAsync returns a Task<HttpResponseMessage>. 
        var urlContents = await response.Content.ReadAsByteArrayAsync();      // Retrieve the website contents from the HttpResponseMessage.
        feed.LatestRssXml = System.Text.Encoding.Default.GetString(urlContents);
        feed.Note = "Done";
      }
      catch (Exception ex)
      {
        feed.Note = ex.Message;
        feed.LatestRssXml = ex.ToString();
      }
      return feed;
    }
    ObservableCollection<FeedNpc> loadFeedList()
    {
      var rv = new ObservableCollection<FeedNpc>();
      foreach (var r in _dbx.Feeds.Where(k => k.IsActive))//&& string.Compare(k.HostMachineId , Environment.MachineName, true) == 0).Take(113))
      { rv.Add(new FeedNpc { Id = r.Id, Name = r.Name, Url = r.Url }); }
      return rv;
    }

  }
}
