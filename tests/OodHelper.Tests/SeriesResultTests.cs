using System;
using System.Collections.Generic;
using System.Linq;
using OodHelper;
using Xunit;

namespace OodHelper.Tests
{
    /// <summary>
    /// Characterization tests for the pure <see cref="SeriesResult"/> scoring algorithm (totals,
    /// discards, DNC and average-score handling). They pin the numeric behaviour so the EF/MVVM
    /// migration of the series scoring/display pass is provably behaviour-preserving. The algorithm
    /// touches no database — it operates purely on the per-event entries handed to it.
    /// </summary>
    public class SeriesResultTests
    {
        private static readonly DateTime R1 = new DateTime(2024, 1, 1);
        private static readonly DateTime R2 = new DateTime(2024, 1, 8);
        private static readonly DateTime R3 = new DateTime(2024, 1, 15);

        private static SeriesEntry Entry(int bid, int rid, DateTime date, string code, double? points)
            => new SeriesEntry { bid = bid, rid = rid, date = date, code = code, points = points };

        private static SeriesEntry Finisher(int bid, int rid, DateTime date, double points)
            => Entry(bid, rid, date, string.Empty, points);

        private static Dictionary<int, SeriesEvent> Series(
            params (int rid, DateTime date, SeriesEntry[] entries)[] races)
        {
            var data = new Dictionary<int, SeriesEvent>();
            foreach (var (rid, date, entries) in races)
            {
                var ev = new SeriesEvent(rid, date);
                foreach (var e in entries)
                    ev.AddEntry(e);
                data[rid] = ev;
            }
            return data;
        }

        private static BoatSeriesResult ResultFor(SeriesResult sr, int bid)
            => sr.Results.Single(r => r.Bid == bid);

        [Fact]
        public void TwoBoats_NoDiscards_SumsPointsAndAssignsPlaces()
        {
            var data = Series(
                (1, R1, new[] { Finisher(1, 1, R1, 1), Finisher(2, 1, R1, 2) }),
                (2, R2, new[] { Finisher(1, 2, R2, 1), Finisher(2, 2, R2, 2) }));

            var sr = new SeriesResult(1, "Fast", data, new[] { 0, 0 });
            sr.Score();

            var a = ResultFor(sr, 1);
            var b = ResultFor(sr, 2);

            Assert.Equal(2.0, a.Net);
            Assert.Equal(2.0, a.Total);
            Assert.Equal(2, a.Count);
            Assert.Equal(1, a.Place);

            Assert.Equal(4.0, b.Net);
            Assert.Equal(2, b.Place);
        }

        [Fact]
        public void DiscardProfile_DropsWorstRaceFromNetButNotGross()
        {
            // profile {0,1}: with 3 races, profile.Length(2) < count(3) -> use last (1 discard).
            var data = Series(
                (1, R1, new[] { Finisher(1, 1, R1, 1), Finisher(2, 1, R1, 2) }),
                (2, R2, new[] { Finisher(1, 2, R2, 1), Finisher(2, 2, R2, 2) }),
                (3, R3, new[] { Finisher(1, 3, R3, 5), Finisher(2, 3, R3, 2) }));

            var sr = new SeriesResult(1, "Fast", data, new[] { 0, 1 });
            sr.Score();

            var a = ResultFor(sr, 1);
            var b = ResultFor(sr, 2);

            // A discards its 5 -> net 1+1=2, gross still 1+1+5=7.
            Assert.Equal(2.0, a.Net);
            Assert.Equal(7.0, a.Total);
            // The dropped race is flagged for the grey-out in the grid.
            Assert.True(data[3].Entrants[1].discard);

            // B discards one of its 2s -> net 2+2=4, gross 2+2+2=6.
            Assert.Equal(4.0, b.Net);
            Assert.Equal(6.0, b.Total);

            Assert.Equal(1, a.Place);
            Assert.Equal(2, b.Place);
        }

        [Fact]
        public void MissedRace_ScoresDncAtEntrantsPlusOne_AndIsExcludedFromGross()
        {
            // Two boats overall; A only sails race 1. No discards.
            var data = Series(
                (1, R1, new[] { Finisher(1, 1, R1, 1), Finisher(2, 1, R1, 2) }),
                (2, R2, new[] { Finisher(2, 2, R2, 2) }));

            var sr = new SeriesResult(1, "Fast", data, new[] { 0, 0 });
            sr.Score();

            // A gets a DNC in race 2 worth (#boats + 1) = 3.
            var aDnc = data[2].Entrants[1];
            Assert.Equal("DNC", aDnc.code);
            Assert.Equal(3.0, aDnc.Points);

            var a = ResultFor(sr, 1);
            // DNC is excluded from gross (and the entered count), but counts toward net.
            Assert.Equal(1.0, a.Total);
            Assert.Equal(1, a.Count);
            Assert.Equal(4.0, a.Net); // 1 (race1) + 3 (DNC)
        }

        [Fact]
        public void AverageScoreCode_GetsAveragedPoints_AndIsExcludedFromGross()
        {
            // Single boat: race1 = 2, race2 = OOD (average), race3 = 4. No discards.
            var data = Series(
                (1, R1, new[] { Finisher(1, 1, R1, 2) }),
                (2, R2, new[] { Entry(1, 2, R2, "OOD", null) }),
                (3, R3, new[] { Finisher(1, 3, R3, 4) }));

            var sr = new SeriesResult(1, "Fast", data, new[] { 0, 0, 0 });
            sr.Score();

            // Average of the countable, non-average races: (2 + 4) / 2 = 3.
            var ood = data[2].Entrants[1];
            Assert.Equal(3.0, ood.Points);

            var a = ResultFor(sr, 1);
            Assert.Equal(6.0, a.Total);     // gross excludes the OOD race
            Assert.Equal(2, a.Count);
            Assert.Equal(9.0, a.Net);       // 2 + 3 (avg) + 4
        }
    }
}
