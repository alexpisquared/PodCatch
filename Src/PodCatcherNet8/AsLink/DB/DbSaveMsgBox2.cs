//todo: move to shared somewhere (202-12)
//using AAV.Sys.Ext;
//using AAV.Sys.Helpers;
using System.Diagnostics;
using System.Windows;
using AsLink;
using Microsoft.EntityFrameworkCore;

namespace PodCatcherNet8.AsLink.DB;

public static class DbSaveMsgBox_OldRestoredInDec2023
{
  public static int CheckAskSave(DbContext db, int maxRowsToShow = 32, MessageBoxButton btn = MessageBoxButton.YesNoCancel)
  {
    var rowsSaved = (int)MsgBoxDbRslt.Unknown;
    retry:
    try
    {
      //Bpr.BeepOk();

      if (!db.ChangeTracker.Entries().Any(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        rowsSaved = (int)MsgBoxDbRslt.NoChanges;
      else
        switch (MessageBox.Show(db.GetDbChangesReport(maxRowsToShow), "Save Changes?", btn, MessageBoxImage.Question, MessageBoxResult.Yes))
        {
          case MessageBoxResult.Yes: rowsSaved = TrySaveAsk(db, "Manual Yes"); if (rowsSaved < 0) goto retry; return rowsSaved; // >0;		yes
          case MessageBoxResult.No: return (int)MsgBoxDbRslt.No;                                                  // -1;		no
          case MessageBoxResult.Cancel: return (int)MsgBoxDbRslt.Cancel;                                          // -2;		cancel
        }
    }
    catch (Exception ex)
    {
      ex.Log();
      if (MessageBox.Show(ex.ToString(), "Exception detected - Retry saving?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
        goto retry;
    }

    return rowsSaved;
  }
  public static int TrySaveAsk(DbContext db, string note)
  {
    retry:
    try
    {
      var sw = Stopwatch.StartNew();
      var rowsSaved = db.SaveChanges();
      var rv = $"{rowsSaved,8:N0} rows saved in{sw.Elapsed.TotalSeconds,6:N1}s  <=  {note}";
      Trace.WriteLine(rv);
      return rowsSaved;
    }
    catch (DbEntityValidationException ex)
    {
      var ves = ex.ValidationExceptionToString();
      Trace.WriteLine(ves);
      if (Debugger.IsAttached) Debugger.Break();
      else if (MessageBox.Show(ves, "Exceptions detected - Retry saving?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
        goto retry;
    }
    catch (DbUpdateException ex)
    {
      var ves = ex.DbUpdateExceptionToString();
      Trace.WriteLine(ves);
      if (Debugger.IsAttached) Debugger.Break();
      else if (MessageBox.Show(ves, "Exceptions detected - Retry saving?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
        goto retry;
    }
    catch (Exception ex)
    {
      Trace.WriteLine(ex.InnermostMessage());
      if (Debugger.IsAttached) Debugger.Break();
      if (MessageBox.Show(ex.InnermostMessage(), "Exception detected - Retry saving?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
        goto retry;
    }

    return -2;
  }
}

public enum MsgBoxDbRslt // MsgBoxReverseRslt
{
  OK = -MessageBoxResult.OK,         // -1
  No = -MessageBoxResult.No,         // -7
  Yes = -MessageBoxResult.Yes,       // -6
  Cancel = -MessageBoxResult.Cancel, // -2
  Unknown = -3,
  NoChanges = -4,
}
