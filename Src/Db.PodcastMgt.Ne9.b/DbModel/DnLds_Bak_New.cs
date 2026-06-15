namespace Db.PodcastMgt.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DnLds_Bak_New
    {
        [Key]
        [Column(Order = 0)]
        public int Id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FeedId { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime PublishedAt { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(256)]
        public string CastTitle { get; set; }

        [StringLength(1024)]
        public string CastSummary { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(320)]
        public string CastUrl { get; set; }

        public long? CastFileLength { get; set; }

        [Key]
        [Column(Order = 5)]
        public string CastFilenameExt { get; set; }

        [StringLength(22)]
        public string TrgFileSortPrefix { get; set; }

        [Key]
        [Column(Order = 6)]
        public DateTime RowAddedAt { get; set; }

        [StringLength(22)]
        public string RowAddedByPC { get; set; }

        public DateTime? DownloadStart { get; set; }

        public DateTime? DownloadedAt { get; set; }

        public long? DownloadedLength { get; set; }

        [StringLength(22)]
        public string DownloadedByPC { get; set; }

        [StringLength(256)]
        public string DownloadedToDir { get; set; }

        [Key]
        [Column(Order = 7)]
        public bool ReDownload { get; set; }

        [Key]
        [Column(Order = 8)]
        public bool IsDownloading { get; set; }

        public double? DurationMin { get; set; }

        [StringLength(1)]
        public string DnldStatusId { get; set; }

        [StringLength(1024)]
        public string Note { get; set; }
    }
}
