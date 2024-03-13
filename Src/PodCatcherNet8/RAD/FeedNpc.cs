using MVVM.Common;
using System;

namespace PodCatcher.RAD
{
  public class FeedNpc : BindableBase
  {
    public FeedNpc() { }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Tla { get; set; }
    public string SubFolder { get; set; }
    public string Note                            /**/
                                  { get => _Note; set => Set(ref _Note, value); }
    string _Note = null;
    public string Url { get; set; }
    public string LatestRssXml                    /**/
                          { get => _LatestRssXml; set => Set(ref _LatestRssXml, value); }
    string _LatestRssXml = null;
    public string LatestRssText                   /**/
                         { get => _LatestRssText; set => Set(ref _LatestRssText, value); }
    string _LatestRssText = null;
    public string StatusInfo { get; set; }
    public bool IsActive { get; set; }
    public bool IsActiveDbg { get; set; }
    public int KbPerMin { get; set; }
    public int AdOffsetSec { get; set; }
    public int AcptblAgeDay { get; set; }
    public DateTime IgnoreBefore { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public bool IsLastCheckSuccessful             /**/
                   { get => _IsLastCheckSuccessful; set => Set(ref _IsLastCheckSuccessful, value); }
    bool _IsLastCheckSuccessful = false;
    public string LastCheckedPC { get; set; }
    public int? LastAvailCastCount { get; set; }
    public DateTime? LastCastAt { get; set; }
    public bool IsDeleted { get; set; }
    public string HostMachineId { get; set; }
    //public virtual ICollection<DnLd> DnLds { get; set; }
    //public virtual ICollection<DnLds_BAD> DnLds_BAD { get; set; }
    //public virtual Machine Machine { get; set; }
  }
}
