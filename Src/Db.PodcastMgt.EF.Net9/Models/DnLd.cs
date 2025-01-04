using System;
using System.Collections.Generic;

namespace Db.PodcastMgt.EF.Net9.Models;

public partial class DnLd
{
    public int Id { get; set; }

    public int FeedId { get; set; }

    public DateTime PublishedAt { get; set; }

    public string CastTitle { get; set; } = null!;

    public string? CastSummary { get; set; }

    public string CastUrl { get; set; } = null!;

    public long? CastFileLength { get; set; }

    public string CastFilenameExt { get; set; } = null!;

    public string? TrgFileSortPrefix { get; set; }

    public DateTime RowAddedAt { get; set; }

    public string? RowAddedByPc { get; set; }

    public DateTime? DownloadStart { get; set; }

    public DateTime? DownloadedAt { get; set; }

    public long? DownloadedLength { get; set; }

    public string? DownloadedByPc { get; set; }

    public string? DownloadedToDir { get; set; }

    public bool ReDownload { get; set; }

    public bool IsDownloading { get; set; }

    public double? DurationMin { get; set; }

    public string? DnldStatusId { get; set; }

    public string? Note { get; set; }

    public string? ErrLog { get; set; }

    public DateTime? AvailableLastDate { get; set; }

    public bool IsStillOnline { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual DnldStatus? DnldStatus { get; set; }

    public virtual Feed Feed { get; set; } = null!;
}
