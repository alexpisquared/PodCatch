namespace Db.PodcastMgt.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class DnLds_BAD
    {
        public int Id { get; set; }

        public int FeedId { get; set; }

        [Required]
        public string CastTitle { get; set; }

        [Required]
        public string CastUrl { get; set; }

        [Required]
        public string CastFilenameExt { get; set; }

        public long? CastFileLength { get; set; }

        public DateTime PublishedAt { get; set; }

        public string TrgFileSortPrefix { get; set; }

        public DateTime RowAddedAt { get; set; }

        public string RowAddedByPC { get; set; }

        public DateTime? DownloadedAt { get; set; }

        public string DownloadedByPC { get; set; }

        public string DownloadedToDir { get; set; }

        public long? DownloadedLength { get; set; }

        public bool ReDownload { get; set; }

        public bool IsDownloading { get; set; }

        public double? DurationMin { get; set; }

        public virtual Feed Feed { get; set; }
    }
}
