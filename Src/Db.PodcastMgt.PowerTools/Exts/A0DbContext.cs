using Microsoft.EntityFrameworkCore;
namespace Db.PodcastMgt.PowerTools.Exts;
public partial class PodcastMgtContext : DbContext
{
  public static PodcastMgtContext Create() => new();

  PodcastMgtContext()
    : base()
  {
    //Database.Connection.ConnectionString = conStr; //tu: remove CREATE TABLE permissin neeed - Database.SetInitializer<PodcastMgtContext>(null);
    //Trace.WriteLine(string.Format("  SqlEnv: {0}. \r\n ConStr: {1}.", SqlEnv, Database.Connection.ConnectionString));
  }

  static string conStr
  {
    get {
      var dbg =
#if DEBUG______
        "Dbg";
#else
"";
#endif
      switch (Environment.MachineName) // .Equals("Asus2", StringComparison.OrdinalIgnoreCase))
      {
        default:
        case "AP-201704-LW":
        case "Asus2":        /**/ return $@"data source=.\sqlexpress;               initial catalog=PodcastMgt{dbg}; integrated security=True;MultipleActiveResultSets=True;App=EntityFramework;Encrypt=False;Connect Timeout=180";
        case "AP-201704-L~": /**/ return $@"data source={Environment.MachineName};  initial catalog=PodcastMgt{dbg}; integrated security=True;MultipleActiveResultSets=True;App=EntityFramework;Encrypt=False;Connect Timeout=180";
      }
    }
  }

  public static string SqlEnv =>
#if DEBUG
      "dbg";
#else
              return "RLS";
#endif
}