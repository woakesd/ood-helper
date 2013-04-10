using System;
namespace OodHelper.Results.Model
{
    public interface ICalendarEvent
    {
        string course { get; set; }
        string eventClass { get; }
        string eventName { get; }
        int? extension { get; set; }
        CalendarEvent.Handicappings handicapping { get; set; }
        int? laps { get; set; }
        string ood { get; set; }
        CalendarEvent.RaceTypes racetype { get; set; }
        int rid { get; set; }
        DateTime? start_date { get; set; }
        int? time_limit_delta { get; set; }
        DateTime? time_limit_fixed { get; set; }
        CalendarEvent.TimeLimitTypes time_limit_type { get; set; }
        void Update();
        string wind_direction { get; set; }
        string wind_speed { get; set; }
        double? standard_corrected_time { get; set; }
    }
}
