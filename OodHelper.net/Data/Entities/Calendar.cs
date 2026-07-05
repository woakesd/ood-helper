using System;
using System.Collections.Generic;

namespace OodHelper.Data.Entities;

public partial class Calendar
{
    public int Rid { get; set; }

    public DateTime? StartDate { get; set; }

    public string?TimeLimitType { get; set; }

    public DateTime? TimeLimitFixed { get; set; }

    public int? TimeLimitDelta { get; set; }

    public int? Extension { get; set; }

    public string?Class { get; set; }

    public string?Event { get; set; }

    public string?PriceCode { get; set; }

    public string?Course { get; set; }

    public string?Ood { get; set; }

    public string?Venue { get; set; }

    public string?Racetype { get; set; }

    public string?Handicapping { get; set; }

    public int? Visitors { get; set; }

    public string?Flag { get; set; }

    public string?Memo { get; set; }

    public bool? IsRace { get; set; }

    public bool? Raced { get; set; }

    public bool? Approved { get; set; }

    public string?CourseChoice { get; set; }

    public int? LapsCompleted { get; set; }

    public string?WindSpeed { get; set; }

    public string?WindDirection { get; set; }

    public double? StandardCorrectedTime { get; set; }

    public DateTime? ResultCalculated { get; set; }

    public virtual ICollection<Race> Races { get; set; } = new List<Race>();

    public virtual ICollection<Series> Sids { get; set; } = new List<Series>();
}
