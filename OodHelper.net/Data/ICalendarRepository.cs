using System;
using System.Collections.Generic;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    /// <summary>
    /// EF Core data access for race (calendar) maintenance: the <c>calendar</c> table behind the
    /// Races list and the per-race editor.
    /// </summary>
    public interface ICalendarRepository
    {
        /// <summary>All calendar rows ordered by date, optionally filtered by event name.</summary>
        IReadOnlyList<Calendar> GetAll(string filter);
        Calendar? Get(int rid);
        /// <summary>Insert or update a calendar row. Returns the (possibly new) rid.</summary>
        int Save(Calendar calendar);
        void Delete(int rid);
        /// <summary>Updates only the memo (race notes) for one race, leaving every other column untouched.</summary>
        void UpdateMemo(int rid, string? memo);

        /// <summary>
        /// The latest race start date on or before <paramref name="onOrBefore"/>, or null if there are
        /// no races in range. Backs the RaceChooser default-date selection.
        /// </summary>
        DateTime? GetLatestRaceDate(DateTime onOrBefore);

        /// <summary>
        /// The distinct calendar days (date only, ordered) on which races are scheduled, optionally
        /// filtered by event name. Backs the RaceChooser calendar's blackout/display range.
        /// </summary>
        IReadOnlyList<DateTime> GetRaceDays(string filter);

        /// <summary>Races whose start falls on <paramref name="day"/>, ordered by start time.</summary>
        IReadOnlyList<Calendar> GetRacesOnDay(DateTime day);

        /// <summary>
        /// Upcoming, not-yet-raced races starting within <paramref name="days"/> days (inclusive) of
        /// <paramref name="from"/>, ordered by start. Backs the entry-sheet print selector.
        /// </summary>
        IReadOnlyList<Calendar> GetUpcomingUnraced(DateTime from, int days);
    }
}
