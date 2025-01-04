using System;
using System.Collections.Generic;

namespace Db.PodcastMgt.EF.Net9.Models;

public partial class Feed
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Tla { get; set; }

    public string? SubFolder { get; set; }

    public string? Note { get; set; }

    public string Url { get; set; } = null!;

    public string? LatestRssXml { get; set; }

    public string? LatestRssText { get; set; }

    public string StatusInfo { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsActiveDbg { get; set; }

    public int CastQntNew { get; set; }

    public int CastQntTtl { get; set; }

    public int KbPerMin { get; set; }

    public int AdOffsetSec { get; set; }

    public int AcptblAgeDay { get; set; }

    public DateTime IgnoreBefore { get; set; }

    public DateTime AddedAt { get; set; }

    public DateTime? LastCheckedAt { get; set; }

    public string? LastCheckedPc { get; set; }

    public int? LastAvailCastCount { get; set; }

    public DateTime? LastCastAt { get; set; }

    public bool IsDeleted { get; set; }

    public string? HostMachineId { get; set; }

    public int PriorityGroup { get; set; }

    public bool IsTitleInFilename { get; set; }

    public double PartSizeMin { get; set; }

    public bool? IsNewerFirst { get; set; }

    public virtual ICollection<DnLd> DnLds { get; set; } = new List<DnLd>();

    public virtual Machine? HostMachine { get; set; }
}
