using System;
using System.Collections.Generic;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    /// <summary>
    /// EF Core data access for the race-results editor (calendar header + races rows).
    /// Replaces the inline <c>Db</c> SQL that used to live in <c>ResultsEditor</c> /
    /// <c>RaceResults</c>. The handicap scoring engines (<see cref="IRaceScore"/>) remain on the
    /// legacy <c>Db</c> helper for now, so this repository deliberately mirrors the existing
    /// behaviour rather than re-deriving it.
    /// </summary>
    public interface IRaceResultsRepository
    {
        // --- Reads -------------------------------------------------------------------------
        Calendar GetCalendar(int rid);
        IReadOnlyList<Race> GetRaceRows(int rid);

        /// <summary>Mirrors the old "result not yet calculated, or edited since" check.</summary>
        bool GetCalculateEnabled(int rid);

        /// <summary>True when at least one boat has a newer rolling handicap available.</summary>
        bool GetRefreshHandicapsEnabled(int rid);

        /// <summary>Count of boats that have raced elsewhere in the same series (auto-populate).</summary>
        int CountAutoPopulate(int rid);

        // --- Writes (immediate, per the agreed persistence model) --------------------------
        void UpdateRaceRow(Race row);

        void UpdateCourse(int rid, string course);
        void UpdateWindSpeed(int rid, string windSpeed);
        void UpdateWindDirection(int rid, string windDirection);
        void UpdateLaps(int rid, int? laps);
        void UpdateRaceType(int rid, string raceType);

        /// <summary>Writes the new start date/time to every race row and the calendar.</summary>
        void UpdateStartDate(int rid, DateTime startDate);
        void UpdateTimeLimitFixed(int rid, DateTime timeLimitFixed);
        void UpdateTimeLimitDelta(int rid, int timeLimitDeltaSeconds);
        void UpdateExtension(int rid, int extensionSeconds);

        // --- Bulk operations ---------------------------------------------------------------
        void DoAutoPopulate(int rid);
        void RefreshRollingHandicaps(int rid);
        void MoveToFleet(int fromRid, int toRid, int bid);

        /// <summary>Copies the boat's current handicaps onto this race entry (Edit Boat).</summary>
        void ApplyEditedBoatHandicaps(int rid, int bid);
    }
}
