using System;
using System.Collections.Generic;

namespace OodHelper.Data.Entities;

public partial class SelectRule
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Guid? Parent { get; set; }

    public int? Application { get; set; }

    public string Field { get; set; }

    public int? Condition { get; set; }

    public string StringValue { get; set; }

    public decimal? NumberBound1 { get; set; }

    public decimal? NumberBound2 { get; set; }
}
