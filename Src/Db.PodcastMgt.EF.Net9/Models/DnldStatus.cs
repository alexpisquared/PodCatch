using System;
using System.Collections.Generic;

namespace Db.PodcastMgt.EF.Net9.Models;

public partial class DnldStatus
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<DnLd> DnLds { get; set; } = new List<DnLd>();
}
