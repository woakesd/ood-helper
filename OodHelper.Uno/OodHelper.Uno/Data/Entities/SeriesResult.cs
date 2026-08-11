using System;
using System.Collections.Generic;

namespace OodHelper.Data.Entities;

public partial class SeriesResult
{
    public int Sid { get; set; }

    public int Bid { get; set; }

    public string Division { get; set; } = null!;

    public int? Entered { get; set; }

    public double? Gross { get; set; }

    public double? Nett { get; set; }

    public int? Place { get; set; }
}
