using System;
using System.Collections.Generic;

namespace Db.PodcastMgt.EF.Net9.Models;

public partial class Machine
{
    public string Id { get; set; } = null!;

    public bool IsUsable { get; set; }

    public string? Note { get; set; }

    public string TargetDrive { get; set; } = null!;

    public virtual ICollection<Feed> Feeds { get; set; } = new List<Feed>();
}
