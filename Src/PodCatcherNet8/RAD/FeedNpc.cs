namespace PodCatcherNet8.RAD;
public class FeedNpc : CommunityToolkit.Mvvm.ComponentModel.ObservableValidator
{
  public FeedNpc() { }
  public int Id { get; set; }
  public string Name { get; set; }
  public string Tla { get; set; }
  public string SubFolder { get; set; }
  public string Note { get; set; }
  public string Url { get; set; }
  public string LatestRssXml { get; set; }
  public string LatestRssText { get; set; }
  public string StatusInfo { get; set; }
  public bool IsActive { get; set; }
  public bool IsActiveDbg { get; set; }
  public int KbPerMin { get; set; }
  public int AdOffsetSec { get; set; }
  public int AcptblAgeDay { get; set; }
  public DateTime IgnoreBefore { get; set; }
  public DateTime AddedAt { get; set; }
  public DateTime? LastCheckedAt { get; set; }
  public bool IsLastCheckSuccessful { get; set; }
  public string LastCheckedPC { get; set; }
  public int? LastAvailCastCount { get; set; }
  public DateTime? LastCastAt { get; set; }
  public bool IsDeleted { get; set; }
  public string HostMachineId { get; set; }
  //public virtual ICollection<DnLd> DnLds { get; set; }
  //public virtual ICollection<DnLds_BAD> DnLds_BAD { get; set; }
  //public virtual Machine Machine { get; set; }
}
