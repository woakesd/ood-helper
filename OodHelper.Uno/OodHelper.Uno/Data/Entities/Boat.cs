using System;
using System.Collections.Generic;

namespace OodHelper.Data.Entities;

public partial class Boat
{
    public int Bid { get; set; }

    public string? Boatname { get; set; }

    public string? Boatclass { get; set; }

    public string? Sailno { get; set; }

    public bool? Dinghy { get; set; }

    public string? Hulltype { get; set; }

    public int? Distance { get; set; }

    public int? OpenHandicap { get; set; }

    public string? HandicapStatus { get; set; }

    public int? RollingHandicap { get; set; }

    public int? CrewSkillFactor { get; set; }

    public decimal? SmallCatHandicapRating { get; set; }

    public string? EnginePropeller { get; set; }

    public string? Keel { get; set; }

    public string? Deviations { get; set; }

    public string? Subscription { get; set; }

    public string? Boatmemo { get; set; }

    public string? Berth { get; set; }

    public bool? Hired { get; set; }

    public string? P { get; set; }

    public bool? S { get; set; }

    public int? Beaten { get; set; }

    public Guid? Uid { get; set; }

    public virtual ICollection<Race> Races { get; set; } = new List<Race>();
}
