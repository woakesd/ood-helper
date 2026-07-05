using System;
using System.Collections.Generic;
using System.Linq;
using OodHelper;
using OodHelper.Data.Entities;
using OodHelper.Results;
using OodHelper.Results.Scoring;
using Xunit;

namespace OodHelper.Tests
{
    /// <summary>
    /// Characterization tests for the pure scoring core extracted from the old
    /// <c>OpenHandicap</c>/<c>RollingHandicap</c> engines. They pin the numeric behaviour (corrected
    /// times, SCT, places/points, rolling-handicap movement, DNF/time-limit handling) so the EF
    /// migration is provably behaviour-preserving. All handicaps/elapsed times are chosen to produce
    /// clean, hand-checkable numbers.
    /// </summary>
    public class HandicapScoreCalculatorTests
    {
        private static readonly DateTime Start = new DateTime(2024, 1, 1, 10, 0, 0);
        private static readonly DateTime FarTimeLimit = Start.AddHours(5);

        private static Race Row(int bid, int openHandicap, int? rollingHandicap, DateTime? finish,
            int? laps = null, bool restrictedSail = false, string handicapStatus = null)
        {
            return new Race
            {
                Rid = 1,
                Bid = bid,
                StartDate = Start,
                FinishDate = finish,
                OpenHandicap = openHandicap,
                RollingHandicap = rollingHandicap,
                Laps = laps,
                RestrictedSail = restrictedSail,
                HandicapStatus = handicapStatus
            };
        }

        private static HandicapScoreInputs Inputs(CalendarModel.RaceTypes raceType, HandicapMode mode,
            DateTime? timeLimit, int? extension = null, Func<int, DateTime, double?> priorPerf = null)
        {
            return new HandicapScoreInputs
            {
                RaceType = raceType,
                Mode = mode,
                TimeLimit = timeLimit,
                Extension = extension,
                RsCoefficient = 1.04,
                RhCoefficient = 0.15,
                PreviousNewRollingHandicaps = new Dictionary<int, int>(),
                PriorPerformanceLookup = priorPerf ?? ((_, _) => null)
            };
        }

        [Fact]
        public void FixedLengthOpen_ComputesElapsedCorrectedPlacesPointsSctAndHandicaps()
        {
            // elapsed 1000s; opens chosen so corrected = Round(1_000_000 / open):
            //   A open 1000 -> 1000, B open 1020 -> 980, C open 980 -> 1020, D open 500 -> 2000.
            var finish = Start.AddSeconds(1000);
            var a = Row(bid: 1, openHandicap: 1000, rollingHandicap: 1000, finish: finish);
            var b = Row(bid: 2, openHandicap: 1020, rollingHandicap: 1020, finish: finish);
            var c = Row(bid: 3, openHandicap: 980, rollingHandicap: 980, finish: finish);
            var d = Row(bid: 4, openHandicap: 500, rollingHandicap: 500, finish: finish);
            var rows = new List<Race> { a, b, c, d };

            var outcome = HandicapScoreCalculator.Calculate(rows,
                Inputs(CalendarModel.RaceTypes.FixedLength, HandicapMode.Open, FarTimeLimit));

            Assert.True(outcome.HasFinishers);
            Assert.Equal(4, outcome.Finishers);
            Assert.Empty(outcome.Warnings);

            // Elapsed + corrected (open: standard corrected == corrected).
            Assert.Equal(1000, a.Elapsed);
            Assert.Equal(1000.0, a.Corrected);
            Assert.Equal(980.0, b.Corrected);
            Assert.Equal(1020.0, c.Corrected);
            Assert.Equal(2000.0, d.Corrected);
            Assert.Equal(a.Corrected, a.StandardCorrected);

            // Places by corrected ascending: B(980), A(1000), C(1020), D(2000); no ties.
            Assert.Equal(2, a.Place);
            Assert.Equal(1, b.Place);
            Assert.Equal(3, c.Place);
            Assert.Equal(4, d.Place);
            Assert.Equal(2.0, a.Points);
            Assert.Equal(1.0, b.Points);
            Assert.Equal(3.0, c.Points);
            Assert.Equal(4.0, d.Points);

            // SCT: top 2/3 (3 of 4) average = (980+1000+1020)/3 = 1000; all three beat +5% (1050).
            Assert.Equal(1000.0, outcome.StandardCorrectedTime);

            // Achieved handicaps = Round(std / SCT * open); all resolve to 1000 here.
            Assert.Equal(1000, a.AchievedHandicap);
            Assert.Equal(1000, b.AchievedHandicap);
            Assert.Equal(1000, c.AchievedHandicap);
            Assert.Equal(1000, d.AchievedHandicap);

            Assert.Equal(0, a.PerformanceIndex);
            Assert.Equal(-20, b.PerformanceIndex);
            Assert.Equal(20, c.PerformanceIndex);
            Assert.Equal(500, d.PerformanceIndex);

            // New rolling handicaps: move 15% toward achieved, clamped to +/-5%.
            //   A: 1000 + (1000-1000)*.15 = 1000
            //   B: 1020 + (1000-1020)*.15 = 1017
            //   C:  980 + (1000- 980)*.15 =  983
            //   D: exceptional slow (std 2000 >= 1050), no prior slow result -> handicap frozen at 500.
            Assert.Equal(1000, a.NewRollingHandicap);
            Assert.Equal(1017, b.NewRollingHandicap);
            Assert.Equal(983, c.NewRollingHandicap);
            Assert.Equal(500, d.NewRollingHandicap);
            Assert.Equal("s", d.C);
            Assert.Null(a.C);
        }

        [Fact]
        public void ExceptionalSlow_WithPriorSlowResult_AllowsHandicapToMove()
        {
            // Same shape as above but D's prior result was also slow (>5%), so the handicap moves
            // and the flag is promoted to "S".
            var finish = Start.AddSeconds(1000);
            var rows = new List<Race>
            {
                Row(1, 1000, 1000, finish),
                Row(2, 1020, 1020, finish),
                Row(3, 980, 980, finish),
                Row(4, 500, 500, finish)
            };

            var outcome = HandicapScoreCalculator.Calculate(rows,
                Inputs(CalendarModel.RaceTypes.FixedLength, HandicapMode.Open, FarTimeLimit,
                    priorPerf: (bid, _) => bid == 4 ? 10.0 : (double?)null));

            var d = rows.Single(r => r.Bid == 4);
            Assert.Equal("S", d.C);
            // achieved 1000, but clamped to +5% of open (Round(1.05*500)=525) before the 15% move:
            //   500 + (525-500)*.15 = Round(503.75) = 504.
            Assert.Equal(504, d.NewRollingHandicap);
        }

        [Fact]
        public void Ties_SharePlaceAndPoints()
        {
            // B fastest; A and A2 tie on corrected (both open 1000 -> 1000).
            var finish = Start.AddSeconds(1000);
            var b = Row(bid: 1, openHandicap: 1020, rollingHandicap: 1020, finish: finish); // 980
            var a = Row(bid: 2, openHandicap: 1000, rollingHandicap: 1000, finish: finish); // 1000
            var a2 = Row(bid: 3, openHandicap: 1000, rollingHandicap: 1000, finish: finish); // 1000
            var rows = new List<Race> { b, a, a2 };

            HandicapScoreCalculator.Calculate(rows,
                Inputs(CalendarModel.RaceTypes.FixedLength, HandicapMode.Open, FarTimeLimit));

            Assert.Equal(1, b.Place);
            Assert.Equal(2, a.Place);
            Assert.Equal(2, a2.Place);
            // Tied for 2nd: share points for 2nd + 3rd = 2.5 each.
            Assert.Equal(1.0, b.Points);
            Assert.Equal(2.5, a.Points);
            Assert.Equal(2.5, a2.Points);
        }

        [Fact]
        public void RollingMode_UsesRollingForCorrectedAndOpenForStandardCorrected()
        {
            // elapsed 1000s, rolling 900, open 1000.
            //   corrected (rolling)          = Round(1_000_000/900)  = 1111
            //   standard corrected (open)    = Round(1_000_000/1000) = 1000
            var finish = Start.AddSeconds(1000);
            var boat = Row(bid: 1, openHandicap: 1000, rollingHandicap: 900, finish: finish);
            var rows = new List<Race> { boat };

            HandicapScoreCalculator.Calculate(rows,
                Inputs(CalendarModel.RaceTypes.FixedLength, HandicapMode.Rolling, FarTimeLimit));

            Assert.Equal(1111.0, boat.Corrected);
            Assert.Equal(1000.0, boat.StandardCorrected);
            Assert.Equal(1, boat.Place);
        }

        [Fact]
        public void AverageLap_DividesCorrectedByLaps()
        {
            // elapsed 2000s, open 1000, 2 laps -> Round(2_000_000/1000)/2 = 1000.
            var finish = Start.AddSeconds(2000);
            var boat = Row(bid: 1, openHandicap: 1000, rollingHandicap: 1000, finish: finish, laps: 2);
            var rows = new List<Race> { boat };

            HandicapScoreCalculator.Calculate(rows,
                Inputs(CalendarModel.RaceTypes.AverageLap, HandicapMode.Open, FarTimeLimit));

            Assert.Equal(1000.0, boat.Corrected);
        }

        [Fact]
        public void RestrictedSail_DeflatesNewRollingHandicap()
        {
            // Single boat -> SCT can't be computed (<2 good boats), so the only handicap effect is
            // the restricted-sail deflation applied in InitialiseFields: Round(1000/1.04) = 962.
            var finish = Start.AddSeconds(1000);
            var boat = Row(bid: 1, openHandicap: 1000, rollingHandicap: 1000, finish: finish,
                restrictedSail: true);
            var rows = new List<Race> { boat };

            var outcome = HandicapScoreCalculator.Calculate(rows,
                Inputs(CalendarModel.RaceTypes.FixedLength, HandicapMode.Open, FarTimeLimit));

            Assert.Equal(0.0, outcome.StandardCorrectedTime);
            Assert.Equal(962, boat.NewRollingHandicap);
        }

        [Fact]
        public void NoBoatInsideTimeLimit_DoesNotScoreAndWarns()
        {
            // Both boats finish after the time limit -> no finishers, nothing scored.
            var timeLimit = Start.AddSeconds(500);
            var rows = new List<Race>
            {
                Row(1, 1000, 1000, Start.AddSeconds(1000)),
                Row(2, 1000, 1000, Start.AddSeconds(1200))
            };

            var outcome = HandicapScoreCalculator.Calculate(rows,
                Inputs(CalendarModel.RaceTypes.FixedLength, HandicapMode.Open, timeLimit));

            Assert.False(outcome.HasFinishers);
            Assert.Equal(0, outcome.Finishers);
            Assert.Contains(outcome.Warnings, w => w.Contains("outside the timelimit"));
            Assert.Null(rows[0].Place); // untouched
        }

        [Fact]
        public void BoatOutsideTimeLimit_IsMarkedDnfAndWarns()
        {
            var timeLimit = Start.AddSeconds(1500);
            var inside = Row(1, 1000, 1000, Start.AddSeconds(1000));
            var outside = Row(2, 1000, 1000, Start.AddSeconds(2000));
            var rows = new List<Race> { inside, outside };

            var outcome = HandicapScoreCalculator.Calculate(rows,
                Inputs(CalendarModel.RaceTypes.FixedLength, HandicapMode.Open, timeLimit));

            Assert.True(outcome.HasFinishers);
            Assert.Equal(1, outcome.Finishers);
            Assert.Equal("DNF", outside.FinishCode);
            Assert.Contains(outcome.Warnings, w => w.Contains("marked DNF"));
            // The DNF boat is excluded from scoring (kept at the 999 sentinel place).
            Assert.Equal(999, outside.Place);
            Assert.Equal(1, inside.Place);
        }

        [Fact]
        public void Extension_KeepsLateFinisherWithinLimit()
        {
            // boat1 finishes inside the base limit (so there is a finisher and scoring proceeds);
            // boat2 finishes after the base limit but inside the 300s extension, so it is NOT DNF
            // and is still scored. This exercises FlagDidNotFinish's extension handling.
            var timeLimit = Start.AddSeconds(1000);
            var boat1 = Row(1, 1000, 1000, Start.AddSeconds(800));
            var boat2 = Row(2, 1000, 1000, Start.AddSeconds(1200));
            var rows = new List<Race> { boat1, boat2 };

            var outcome = HandicapScoreCalculator.Calculate(rows,
                Inputs(CalendarModel.RaceTypes.FixedLength, HandicapMode.Open, timeLimit, extension: 300));

            Assert.True(outcome.HasFinishers);
            Assert.NotEqual("DNF", boat2.FinishCode);
            Assert.Equal(1, boat1.Place);
            Assert.Equal(2, boat2.Place);
        }
    }
}
