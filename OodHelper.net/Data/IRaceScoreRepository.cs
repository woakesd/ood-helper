using System;
using System.Collections.Generic;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    /// <summary>
    /// Calendar header values the scoring engines need that are not on the race rows. Mirrors the
    /// old grouped header SELECT in <c>OpenHandicap</c> (racetype + the resolved F/D time limit).
    /// </summary>
    public sealed record RaceScoreHeader(
        string RaceType,
        double? StandardCorrectedTime,
        DateTime? TimeLimit,
        int? Extension,
        DateTime? ResultCalculated,
        DateTime? MaxLastEdit);

    /// <summary>
    /// EF Core data access for the handicap scoring engines (open / rolling / stern-chase).
    /// Replaces the inline <c>Db</c> SQL that used to live in <c>OpenHandicap</c>/<c>RollingHandicap</c>/
    /// <c>SternChaseScorer</c>. Complex self-joins are kept as raw SQL (as in
    /// <see cref="RaceResultsRepository"/>) rather than re-derived in LINQ, so behaviour matches the
    /// pure scoring core that consumes it.
    /// </summary>
    public interface IRaceScoreRepository
    {
        // --- Reads -------------------------------------------------------------------------
        RaceScoreHeader GetHeader(int rid);

        /// <summary>Race rows to score (after <see cref="DeleteDidNotCompete"/>), as detached entities.</summary>
        IList<Race> GetScoringRows(int rid);

        /// <summary>
        /// For each boat in the race that has a prior race, that prior race's new rolling handicap.
        /// Boats with no prior race, or whose prior new rolling handicap is null, are absent.
        /// </summary>
        IReadOnlyDictionary<int, int> GetPreviousNewRollingHandicaps(int rid);

        /// <summary>
        /// Performance percentage of the boat's most recent prior qualifying result, or null.
        /// (<c>(achieved - open) / open * 100</c>, latest race with a calculated result before
        /// <paramref name="beforeStart"/>.)
        /// </summary>
        double? GetPriorPerformancePercent(int rid, int bid, DateTime beforeStart);

        // --- Writes ------------------------------------------------------------------------
        void DeleteDidNotCompete(int rid);

        /// <summary>Persists the scored race rows (every scalar column, preserving last_edit).</summary>
        void CommitRows(IEnumerable<Race> rows);

        void UpdateCalendarSct(int rid, double sct);

        /// <summary>Stamps result_calculated = now and raced = 1.</summary>
        void MarkResultCalculated(int rid);

        /// <summary>
        /// Pushes each boat's new rolling handicap onto the boats table, but only when this race is
        /// the boat's latest (the NOT EXISTS "no later race" guard, preserved).
        /// </summary>
        void UpdateBoatRollingHandicaps(int rid, IEnumerable<(int bid, int newRollingHandicap)> handicaps);
    }
}
