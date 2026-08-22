using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Results;
using OodHelper.Services;
using Xunit;

namespace OodHelper.Tests
{
    /// <summary>
    /// Tests for the bulk recalculation: which series it picks up, what the summary counts, and - the
    /// point of the pass - that it re-totals standings without re-scoring any race.
    /// </summary>
    public class SeriesRecalculationServiceTests
    {
        private readonly ISeriesRepository _series = Substitute.For<ISeriesRepository>();
        private readonly ISeriesResultRepository _repo = Substitute.For<ISeriesResultRepository>();
        private readonly IRaceScoreRepository _scoreRepo = Substitute.For<IRaceScoreRepository>();

        private static readonly DateTime R1 = new DateTime(2024, 1, 1);

        private SeriesRecalculationService NewService() =>
            new SeriesRecalculationService(_series, _repo,
                () => new SeriesResultsViewModel(_repo, _scoreRepo));

        private static Series MakeSeries(int sid, string name) => new Series { Sid = sid, Sname = name };

        [Fact]
        public async Task RecalculateAll_TotalsStandings_WithoutRescoringRaces()
        {
            _series.GetAll(string.Empty).Returns(new List<Series> { MakeSeries(5, "Spring") });
            _repo.GetRacesToScore(5).Returns(new List<SeriesRaceToScore>
            {
                new SeriesRaceToScore(1, "AverageLap", "R", "Race 1", "Fast")
            });
            _repo.GetSeriesHeader(5).Returns(new SeriesResultHeader("Spring", "0,0"));
            _repo.GetEntryRows(5).Returns(new List<SeriesEntryRow>
            {
                new SeriesEntryRow("Fast", 1, R1, 1, 1.0, null, null),
                new SeriesEntryRow("Fast", 1, R1, 2, 2.0, null, null)
            });
            _repo.GetBoats(Arg.Any<IReadOnlyCollection<int>>())
                .Returns(new Dictionary<int, BoatDisplayInfo>());

            var summary = await NewService().RecalculateAllAsync(null!, CancellationToken.None);

            Assert.Equal(new SeriesRecalculationSummary(1, 0, 0), summary);
            _repo.Received(1).SaveSeriesResults(5, "Fast",
                Arg.Is<IReadOnlyList<SeriesResultRow>>(rows => rows.Count == 2));

            // No race scoring: the only GetRacesToScore call is the service's own "has results" filter,
            // and nothing reads or writes race points/handicaps.
            _repo.Received(1).GetRacesToScore(5);
            Assert.Empty(_scoreRepo.ReceivedCalls());
        }

        [Fact]
        public async Task RecalculateAll_SkipsSeriesWithNoRacesToScore()
        {
            _series.GetAll(string.Empty).Returns(new List<Series>
            {
                MakeSeries(5, "Spring"), MakeSeries(6, "Empty")
            });
            _repo.GetRacesToScore(5).Returns(new List<SeriesRaceToScore>
            {
                new SeriesRaceToScore(1, "AverageLap", "R", "Race 1", "Fast")
            });
            _repo.GetRacesToScore(6).Returns(new List<SeriesRaceToScore>());
            _repo.GetSeriesHeader(5).Returns(new SeriesResultHeader("Spring", "0,0"));
            _repo.GetEntryRows(5).Returns(new List<SeriesEntryRow>
            {
                new SeriesEntryRow("Fast", 1, R1, 1, 1.0, null, null)
            });
            _repo.GetBoats(Arg.Any<IReadOnlyCollection<int>>())
                .Returns(new Dictionary<int, BoatDisplayInfo>());

            var summary = await NewService().RecalculateAllAsync(null!, CancellationToken.None);

            Assert.Equal(new SeriesRecalculationSummary(1, 1, 0), summary);
            _repo.DidNotReceive().GetEntryRows(6);
        }

        [Fact]
        public async Task RecalculateAll_CountsUnsavedSeriesAsFailed()
        {
            // Build logs a persistence failure instead of throwing, so without SaveFailures this series
            // would be reported as recalculated even though its stored standings never changed.
            _series.GetAll(string.Empty).Returns(new List<Series> { MakeSeries(5, "Spring") });
            _repo.GetRacesToScore(5).Returns(new List<SeriesRaceToScore>
            {
                new SeriesRaceToScore(1, "AverageLap", "R", "Race 1", "Fast")
            });
            _repo.GetSeriesHeader(5).Returns(new SeriesResultHeader("Spring", "0,0"));
            _repo.GetEntryRows(5).Returns(new List<SeriesEntryRow>
            {
                new SeriesEntryRow("Fast", 1, R1, 1, 1.0, null, null)
            });
            _repo.GetBoats(Arg.Any<IReadOnlyCollection<int>>())
                .Returns(new Dictionary<int, BoatDisplayInfo>());
            _repo.When(r => r.SaveSeriesResults(5, "Fast", Arg.Any<IReadOnlyList<SeriesResultRow>>()))
                .Do(_ => throw new InvalidOperationException("database locked"));

            var summary = await NewService().RecalculateAllAsync(null!, CancellationToken.None);

            Assert.Equal(new SeriesRecalculationSummary(0, 0, 1), summary);
        }

        [Fact]
        public async Task RecalculateAll_ReportsProgressWhileScanningForResults()
        {
            _series.GetAll(string.Empty).Returns(new List<Series>
            {
                MakeSeries(5, "Spring"), MakeSeries(6, "Empty")
            });
            _repo.GetRacesToScore(Arg.Any<int>()).Returns(new List<SeriesRaceToScore>());

            var reports = new List<DownloadProgress>();
            await NewService().RecalculateAllAsync(new DelegateProgress(reports.Add),
                CancellationToken.None);

            Assert.Contains(reports, p => p.Message == "Spring: checking for results");
            Assert.Contains(reports, p => p.Message == "Empty: checking for results");
            Assert.All(reports, p => Assert.InRange(p.Percent, 0, 100));
        }

        [Fact]
        public async Task RecalculateAll_ReportsProgressPerSeries()
        {
            _series.GetAll(string.Empty).Returns(new List<Series> { MakeSeries(5, "Spring") });
            _repo.GetRacesToScore(5).Returns(new List<SeriesRaceToScore>
            {
                new SeriesRaceToScore(1, "AverageLap", "R", "Race 1", "Fast")
            });
            _repo.GetSeriesHeader(5).Returns(new SeriesResultHeader("Spring", "0,0"));
            _repo.GetEntryRows(5).Returns(new List<SeriesEntryRow>());

            var reports = new List<DownloadProgress>();
            var progress = new DelegateProgress(reports.Add);

            await NewService().RecalculateAllAsync(progress, CancellationToken.None);

            Assert.Contains(reports, p => p.Message == "Spring: Totalling series results");
        }

        private sealed class DelegateProgress : IProgress<DownloadProgress>
        {
            private readonly Action<DownloadProgress> _onReport;
            public DelegateProgress(Action<DownloadProgress> onReport) => _onReport = onReport;
            public void Report(DownloadProgress value) => _onReport(value);
        }
    }
}
