namespace Db.PodcastMgt.DbModel
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class lkuDnldStatu
    {
        public lkuDnldStatu()
        {
            DnLds = new List<DnLd>();
        }

        [StringLength(1)]
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public virtual ICollection<DnLd> DnLds { get; set; }
    }
}
