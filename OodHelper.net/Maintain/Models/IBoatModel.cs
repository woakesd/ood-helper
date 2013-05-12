using System;

namespace OodHelper.Maintain.Models
{
    public interface IBoatModel
    {
        int? Bid { get; set; }
        string BoatClass { get; set; }
        string BoatName { get; set; }
        string CommitChanges();
        string Deviation { get; set; }
        bool? Dinghy { get; set; }
        string EnginePropeller { get; set; }
        string HandicapStatus { get; set; }
        string HullType { get; set; }
        int? Id { get; set; }
        string Keel { get; set; }
        string Notes { get; set; }
        string OpenHandicap { get; set; }
        string Owner { get; }
        string RollingHandicap { get; set; }
        string SailNumber { get; set; }
        string SmallCatHandicapRating { get; set; }
    }
}
