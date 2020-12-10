namespace Db.PodcastMgt.DbModel
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Machine")]
    public partial class Machine
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Machine()
        {
            Feeds = new HashSet<Feed>();
        }

        [StringLength(15)]
        public string Id { get; set; }

        public bool IsUsable { get; set; }

        [StringLength(500)]
        public string Note { get; set; }

        [Required]
        [StringLength(1)]
        public string TargetDrive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Feed> Feeds { get; set; }
    }
}
