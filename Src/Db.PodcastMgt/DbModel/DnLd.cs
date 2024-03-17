namespace Db.PodcastMgt.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;

  [PropertyChanged.AddINotifyPropertyChangedInterface] // 2024 moved to here from Db.PodcastMgt/Exts/Fody.cs  <== does not work.
  public partial class DnLd
    {
        public int Id { get; set; }

        public int FeedId { get; set; }

        public DateTime PublishedAt { get; set; }

        [Required]
        [StringLength(256)]
        public string CastTitle { get; set; }

        [StringLength(2048)]
        public string CastSummary { get; set; }

        [Required]
        [StringLength(320)]
        public string CastUrl { get; set; }

        public long? CastFileLength { get; set; }

        [Required]
        [StringLength(256)]
        public string CastFilenameExt { get; set; }

        [StringLength(50)]
        public string TrgFileSortPrefix { get; set; }

        public DateTime RowAddedAt { get; set; }

        [StringLength(50)]
        public string RowAddedByPC { get; set; }

        public DateTime? DownloadStart { get; set; }

        public DateTime? DownloadedAt { get; set; }

        public long? DownloadedLength { get; set; }

        [StringLength(50)]
        public string DownloadedByPC { get; set; }

        [StringLength(64)]
        public string DownloadedToDir { get; set; }

        public bool ReDownload { get; set; }

        public bool IsDownloading { get; set; }

        public double? DurationMin { get; set; }

        [StringLength(1)]
        public string DnldStatusId { get; set; }

        [StringLength(2000)]
        public string Note { get; set; }

        [StringLength(2000)]
        public string ErrLog { get; set; }

        public DateTime? AvailableLastDate { get; set; }

        public bool IsStillOnline { get; set; }

        public DateTime ModifiedAt { get; set; }

        public virtual DnldStatu DnldStatu { get; set; }

        public virtual Feed Feed { get; set; }
    }
}
