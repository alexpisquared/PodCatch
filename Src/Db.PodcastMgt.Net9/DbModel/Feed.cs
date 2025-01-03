namespace Db.PodcastMgt.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Feed
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Feed()
        {
            DnLds = new HashSet<DnLd>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(28)]
        public string Tla { get; set; }

        [StringLength(128)]
        public string SubFolder { get; set; }

        [StringLength(2000)]
        public string Note { get; set; }

        [Required]
        [StringLength(256)]
        public string Url { get; set; }

        [Column(TypeName = "xml")]
        public string LatestRssXml { get; set; }

        [Column(TypeName = "text")]
        public string LatestRssText { get; set; }

        [Required]
        [StringLength(32)]
        public string StatusInfo { get; set; }

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

        [StringLength(15)]
        public string LastCheckedPC { get; set; }

        public int? LastAvailCastCount { get; set; }

        public DateTime? LastCastAt { get; set; }

        public bool IsDeleted { get; set; }

        [StringLength(15)]
        public string HostMachineId { get; set; }

        public int PriorityGroup { get; set; }

        public bool IsTitleInFilename { get; set; }

        public double PartSizeMin { get; set; }

        public bool? IsNewerFirst { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DnLd> DnLds { get; set; }

        public virtual Machine Machine { get; set; }
    }
}
