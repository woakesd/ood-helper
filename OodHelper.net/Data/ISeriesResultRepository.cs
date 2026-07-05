using System;
using System.Collections.Generic;

namespace OodHelper.Data
{
    /// <summary>Series header (display name + discard profile) read from the <c>series</c> table.</summary>
    public sealed record SeriesResultHeader(string Name, string? Discards);

    /// <summary>
    /// A race that belongs to the series and needs (re)scoring before the series is totalled.
    /// Mirrors the old grouped calendar/join/races SELECT in <c>RaceSeriesResult</c>.
    /// </summary>
    public sealed record SeriesRaceToScore(int Rid, string? RaceType, string? Handicapping,
        string? EventName, string? ClassName);

    /// <summary>One boat's result row in one race of the series, used to build the per-class table.</summary>
    public sealed record SeriesEntryRow(string? ClassName, int Rid, DateTime StartDate, int Bid,
        double? Points, double? OverridePoints, string? FinishCode);

    /// <summary>A computed boat standing to persist into <c>series_results</c>.</summary>
    public sealed record SeriesResultRow(int Bid, int Entered, double Gross, double Nett, int Place);

    /// <summary>Boat identity columns the results grid shows alongside the scores.</summary>
    public sealed record BoatDisplayInfo(string? Boatname, string? Boatclass, string? Sailno);

    /// <summary>
    /// EF Core data access for the series scoring/display pass. Replaces the inline <c>Db</c> SQL that
    /// used to live in <c>RaceSeriesResult</c>, <c>SeriesResult.SaveResults</c> and
    /// <c>SeriesDisplay</c>. The pure scoring algorithm stays in <see cref="OodHelper.SeriesResult"/>;
    /// this repository only reads the inputs and writes the computed standings.
    /// </summary>
    public interface ISeriesResultRepository
    {
        /// <summary>Series name + discard profile, or null when the sid is unknown.</summary>
        SeriesResultHeader? GetSeriesHeader(int sid);

        /// <summary>
        /// The series' raced races (those with at least one race row), ordered by start date — the
        /// set the screen re-scores before totalling.
        /// </summary>
        IReadOnlyList<SeriesRaceToScore> GetRacesToScore(int sid);

        /// <summary>
        /// Every finished/coded result row across the series, ordered by class, for bucketing into
        /// per-class series events.
        /// </summary>
        IReadOnlyList<SeriesEntryRow> GetEntryRows(int sid);

        /// <summary>Replaces the stored standings for one series/division (delete-then-insert).</summary>
        void SaveSeriesResults(int sid, string division, IReadOnlyList<SeriesResultRow> rows);

        /// <summary>Boat identity columns for the given boats, keyed by bid.</summary>
        IReadOnlyDictionary<int, BoatDisplayInfo> GetBoats(IReadOnlyCollection<int> bids);
    }
}
