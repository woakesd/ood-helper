using System;
using System.Collections.Generic;

namespace OodHelper.Results.Scoring
{
    /// <summary>
    /// Which handicap a race is scored on. The two engines differ <b>only</b> in how corrected
    /// times are derived (see <see cref="HandicapScoreCalculator"/>), so what used to be the
    /// <c>OpenHandicap</c> / <c>RollingHandicap</c> class pair is now a single calculator switched
    /// by this flag.
    /// </summary>
    public enum HandicapMode
    {
        Open,
        Rolling
    }

    /// <summary>
    /// Everything the pure <see cref="HandicapScoreCalculator"/> needs that it cannot derive from
    /// the race rows themselves. Supplied by the orchestrator (<see cref="HandicapScorer"/>) from
    /// the calendar header, <see cref="Settings"/> and the repository, so the calculator stays free
    /// of <c>Db</c>, EF and WPF and is unit-testable in isolation.
    /// </summary>
    public sealed class HandicapScoreInputs
    {
        public CalendarModel.RaceTypes RaceType { get; init; }
        public HandicapMode Mode { get; init; }

        /// <summary>Race time limit (already resolved from the F/D type), or null when none.</summary>
        public DateTime? TimeLimit { get; init; }

        /// <summary>Time-limit extension in seconds, or null.</summary>
        public int? Extension { get; init; }

        /// <summary>Restricted-sail coefficient (<see cref="Settings.RSCoefficieent"/>).</summary>
        public double RsCoefficient { get; init; }

        /// <summary>Rolling-handicap movement coefficient (<see cref="Settings.RHCoefficieent"/>).</summary>
        public double RhCoefficient { get; init; }

        /// <summary>
        /// For boats whose race row has no rolling handicap yet, the new rolling handicap from their
        /// most recent prior race, keyed by bid. Boats with no prior race are absent.
        /// </summary>
        public IReadOnlyDictionary<int, int> PreviousNewRollingHandicaps { get; init; }
            = new Dictionary<int, int>();

        /// <summary>
        /// The most recent prior qualifying result's performance percentage for a boat
        /// (<c>(achieved - open) / open * 100</c>), used to decide whether an exceptional fast/slow
        /// result is allowed to move the handicap. Returns null when there is no such prior result.
        /// Args: bid, and the start date of the current race (only earlier races qualify).
        /// </summary>
        public Func<int, DateTime, double?> PriorPerformanceLookup { get; init; }
            = (_, _) => null;
    }

    /// <summary>Result of a scoring run. The race rows are mutated in place; this carries the scalars.</summary>
    public sealed class HandicapScoreOutcome
    {
        public HandicapScoreOutcome(double standardCorrectedTime, int finishers, bool hasFinishers,
            IReadOnlyList<string> warnings)
        {
            StandardCorrectedTime = standardCorrectedTime;
            Finishers = finishers;
            HasFinishers = hasFinishers;
            Warnings = warnings;
        }

        public double StandardCorrectedTime { get; }
        public int Finishers { get; }

        /// <summary>False when no boat finished inside the time limit (nothing was scored or written).</summary>
        public bool HasFinishers { get; }

        /// <summary>User-facing notices collected during the run (shown by the caller, not here).</summary>
        public IReadOnlyList<string> Warnings { get; }
    }
}
