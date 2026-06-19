using System;
using System.Collections.Generic;

namespace OodHelper.Data.Entities;

public partial class Race
{
    public int Rid { get; set; }

    public int Bid { get; set; }

    public DateTime? StartDate { get; set; }

    public string FinishCode { get; set; }

    public DateTime? FinishDate { get; set; }

    public DateTime? InterimDate { get; set; }

    public bool? RestrictedSail { get; set; }

    public DateTime? LastEdit { get; set; }

    public int? Laps { get; set; }

    public int? Place { get; set; }

    public double? Points { get; set; }

    public double? OverridePoints { get; set; }

    public int? Elapsed { get; set; }

    public double? Corrected { get; set; }

    public double? StandardCorrected { get; set; }

    public string HandicapStatus { get; set; }

    public int? OpenHandicap { get; set; }

    public int? RollingHandicap { get; set; }

    public int? AchievedHandicap { get; set; }

    public int? NewRollingHandicap { get; set; }

    public int? PerformanceIndex { get; set; }

    public string A { get; set; }

    public string C { get; set; }

    public virtual Boat BidNavigation { get; set; }

    public virtual Calendar RidNavigation { get; set; }
}
