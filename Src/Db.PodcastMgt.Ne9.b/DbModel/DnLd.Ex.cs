namespace Db.PodcastMgt.DbModel
{
  using System.ComponentModel.DataAnnotations.Schema;
  using System.IO;

  public partial class DnLd
  {
    [NotMapped]
    public string DnldStatusId_ex { get; set; }
    [NotMapped]
    public string RunTimeNote { get; set; }
    [NotMapped]
    public string SrchD { get; set; }

    [NotMapped]
    public long DownloadedLen => DownloadedLength ?? 0;


    public string FullPathFile(string dir0Org) => Path.Combine(dir0Org, TrgFileSortPrefix + CastFilenameExt);
  }
}
