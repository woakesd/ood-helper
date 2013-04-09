using System;
namespace OodHelper.Results.Model
{
    public interface IEntry
    {
        string A { get; set; }
        string AchievedHandicap { get; set; }
        int Bid { get; }
        string BoatClass { get; }
        string BoatName { get; }
        string C { get; set; }
        string Corrected { get; set; }
        string Elapsed { get; set; }
        string FinishCode { get; set; }
        DateTime? FinishDate { get; set; }
        string FinishTime { get; set; }
        string HandicapStatus { get; set; }
        DateTime? InterimDate { get; set; }
        string InterimTime { get; set; }
        string Laps { get; set; }
        string NewRollingHandicap { get; set; }
        string OpenHandicap { get; set; }
        string OverridePoints { get; set; }
        string PerformanceIndex { get; set; }
        string Place { get; set; }
        string Points { get; set; }
        int Rid { get; }
        string RollingHandicap { get; set; }
        string SailNo { get; }
        string StandardCorrected { get; set; }
        DateTime? StartDate { get; set; }
        string StartTime { get; set; }
    }
}
