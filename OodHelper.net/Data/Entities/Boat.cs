using System;
using System.Collections.Generic;

namespace OodHelper.Data.Entities;

public partial class Boat
{
    public int Bid { get; set; }

    public string? BoatName { get; set; }

    public string? BoatClass { get; set; }

    public string? SailNo { get; set; }

    public bool? Dinghy { get; set; }

    public string? HullType { get; set; }

    public int? OpenHandicap { get; set; }

    public string? HandicapStatus { get; set; }

    public int? RollingHandicap { get; set; }

    public int? CrewSkillFactor { get; set; }

    public decimal? SmallCatHandicapRating { get; set; }

    public string? EnginePropeller { get; set; }

    public string? Keel { get; set; }

    public string? Deviations { get; set; }

    public string? Memo { get; set; }

    public Guid? Uid { get; set; }

    public virtual ICollection<Race> Races { get; set; } = [];
}
