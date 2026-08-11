using System;
using System.Collections.Generic;

namespace OodHelper.Data.Entities;

public partial class Series
{
    public int Sid { get; set; }

    public string Sname { get; set; } = null!;

    public string? Discards { get; set; }

    public virtual ICollection<Calendar> Rids { get; set; } = new List<Calendar>();
}
