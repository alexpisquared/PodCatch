using System.ComponentModel.DataAnnotations.Schema;

namespace Db.PodcastMgt.PowerTools.Models;

  public partial class DnLd
{
  [NotMapped]
  public required string DnldStatusId_ex { get; set; }
  [NotMapped]
  public required string RunTimeNote { get; set; }
  [NotMapped]
  public required string SrchD { get; set; }

  [NotMapped]
  public long DownloadedLen => DownloadedLength ?? 0;


  public string FullPathFile(string dir0Org) => Path.Combine(dir0Org, TrgFileSortPrefix + CastFilenameExt);
}