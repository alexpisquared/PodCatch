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
}