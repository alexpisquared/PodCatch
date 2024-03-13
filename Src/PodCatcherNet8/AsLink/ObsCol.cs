using StandardLib.Extensions;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace PodCatcherNet8.AsLink;

public static class ObservableCollectionEx
{
  public static void ClearAddRangeAsync<T>(this Collection<T> trg, IEnumerable<T> src)
  {
    if (src == null)
    {
      trg.Clear();
      return;
    }

    try
    {
      var dispatcher = Application.Current.Dispatcher; // dispatcher on the UI thread
      _ = dispatcher.BeginInvoke(new Action(() =>
      {
        trg.ClearAddRangeSynch(src); // trg.Clear(); src.ToList().ForEach(trg.Add);
      }));
    }
    catch (Exception ex) { ex.Log(); throw; }
  }
  public static void ClearAddRangeSynch<T>(this Collection<T> trg, IEnumerable<T> src)
  {
    try
    {
      trg.Clear();
      if (src == null) return;        //Trace.WriteLine("List.ToString: {0} ==> Count: {1}.", src.ToString(), src.Count());
      var lst = src.ToList();         //Trace.WriteLine("Adding {0:N0} items...", trg.Count);
      lst.ForEach(trg.Add);
    }
    catch (Exception ex) { ex.Log(); throw; }
  }
  public static void ClearAddRangeAuto<T>(this Collection<T> trg, IEnumerable<T> src)
  {
    try
    {
      if (Application.Current.Dispatcher.CheckAccess()) { trg.Clear(); src?.ToList().ForEach(trg.Add); }// if on UI thread
      else
        _ = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { trg.Clear(); src?.ToList().ForEach(trg.Add); }));

    }
    catch (Exception ex) { ex.Log(); throw; }
  }
  public static void ClearAddRangeAuto2<T>(this Collection<T> trg, IEnumerable<T> src)
  {
    try
    {
      dispatchIfNecessary(() => // <= action
      {
        trg.Clear();
        src?.ToList().ForEach(trg.Add);
      });
    }
    catch (Exception ex) { ex.Log(); throw; }
  }

  static void dispatchIfNecessary(Action action)
  {
    try
    {
      if (!Application.Current.Dispatcher.CheckAccess())  // if on UI thread
        _ = Application.Current.Dispatcher.BeginInvoke(action);
      else
        action.Invoke();
    }
    catch (Exception ex) { ex.Log(); throw; }
  }
}
