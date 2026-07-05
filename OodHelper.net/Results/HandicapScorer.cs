using System;
using System.Collections.Generic;
using System.Linq;
using OodHelper.Data;
using OodHelper.Results.Scoring;

namespace OodHelper.Results
{
    /// <summary>
    /// Open / rolling handicap scoring engine. Replaces the old <c>OpenHandicap</c> /
    /// <c>RollingHandicap</c> class pair: it loads the race via <see cref="IRaceScoreRepository"/>,
    /// runs the pure <see cref="HandicapScoreCalculator"/>, and persists the result. The two modes
    /// share everything except the corrected-time formula (handled inside the calculator), so the
    /// behaviour is selected by <see cref="HandicapMode"/> rather than a subclass.
    /// </summary>
    internal sealed class HandicapScorer : IRaceScore
    {
        private readonly IRaceScoreRepository _repo;
        private readonly HandicapMode _mode;

        public HandicapScorer(IRaceScoreRepository repo, HandicapMode mode)
        {
            _repo = repo;
            _mode = mode;
        }

        public double StandardCorrectedTime { get; private set; }
        public int Finishers { get; private set; }
        public IReadOnlyList<string> Warnings { get; private set; } = Array.Empty<string>();

        public void Calculate(int rid)
        {
            try
            {
                var header = _repo.GetHeader(rid);
                if (header == null) return;

                //
                // Default to the stored SCT (what the old code left exposed when no recalculation
                // happened); overwritten below if we actually score.
                //
                StandardCorrectedTime = header.StandardCorrectedTime ?? 0;

                //
                // Gate: only (re)calculate when results were never calculated, or a row has been
                // edited since the last calculation.
                //
                var shouldCalculate = header.ResultCalculated == null
                    || (header.MaxLastEdit != null && header.ResultCalculated <= header.MaxLastEdit);
                if (!shouldCalculate) return;

                _repo.DeleteDidNotCompete(rid);
                var rows = _repo.GetScoringRows(rid);

                var raceType = Enum.TryParse(header.RaceType, out CalendarModel.RaceTypes rt)
                    ? rt
                    : CalendarModel.RaceTypes.Undefined;

                var inputs = new HandicapScoreInputs
                {
                    RaceType = raceType,
                    Mode = _mode,
                    TimeLimit = header.TimeLimit,
                    Extension = header.Extension,
                    RsCoefficient = Settings.RSCoefficieent,
                    RhCoefficient = Settings.RHCoefficieent,
                    PreviousNewRollingHandicaps = _repo.GetPreviousNewRollingHandicaps(rid),
                    PriorPerformanceLookup = (bid, beforeStart) =>
                        _repo.GetPriorPerformancePercent(rid, bid, beforeStart)
                };

                var outcome = HandicapScoreCalculator.Calculate(rows, inputs);
                Finishers = outcome.Finishers;
                Warnings = outcome.Warnings;

                if (!outcome.HasFinishers)
                    return;

                StandardCorrectedTime = outcome.StandardCorrectedTime;

                _repo.CommitRows(rows);
                _repo.UpdateCalendarSct(rid, outcome.StandardCorrectedTime);
                _repo.UpdateBoatRollingHandicaps(rid,
                    rows.Where(r => r.Place != 999)
                        .Select(r => (r.Bid, r.NewRollingHandicap.Value)));
                _repo.MarkResultCalculated(rid);
            }
            catch (Exception ex)
            {
                //
                // The old engine logged and showed a MessageBox, then swallowed. We keep the
                // swallow (so the print path is unaffected) but surface the message as a warning so
                // the editor can report it via IDialogService.
                //
                ErrorLogger.LogException(ex);
                Warnings = new[] { ex.Message };
            }
        }
    }
}
