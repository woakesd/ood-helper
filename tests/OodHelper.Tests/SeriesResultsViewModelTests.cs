using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Results;
using Xunit;

namespace OodHelper.Tests
{
    /// <summary>
    /// Orchestration tests for <see cref="SeriesResultsViewModel.Build"/>: that it totals each class
    /// from the repository's entry rows, persists the standings and builds the per-class display
    /// view-models. The scoring maths itself is pinned by <see cref="SeriesResultTests"/>, so these
    /// tests use a series with no races-to-score (the re-score loop is exercised by the live scorers).
    /// </summary>
    public class SeriesResultsViewModelTests
    {
        private readonly ISeriesResultRepository _repo = Substitute.For<ISeriesResultRepository>();
        private readonly IRaceScoreRepository _scoreRepo = Substitute.For<IRaceScoreRepository>();

        private static readonly DateTime R1 = new DateTime(2024, 1, 1);
        private static readonly DateTime R2 = new DateTime(2024, 1, 8);

        private SeriesResultsViewModel Build(int sid)
        {
            var vm = new SeriesResultsViewModel(_repo, _scoreRepo);
            vm.Build(sid, null, CancellationToken.None);
            return vm;
        }

        [Fact]
        public void Build_TotalsClass_PersistsAndBuildsDisplay()
        {
            _repo.GetSeriesHeader(5).Returns(new SeriesResultHeader("Spring", "0,0"));
            _repo.GetRacesToScore(5).Returns(new List<SeriesRaceToScore>());
            _repo.GetEntryRows(5).Returns(new List<SeriesEntryRow>
            {
                new SeriesEntryRow("Fast", 1, R1, 1, 1.0, null, null),
                new SeriesEntryRow("Fast", 1, R1, 2, 2.0, null, null),
                new SeriesEntryRow("Fast", 2, R2, 1, 1.0, null, null),
                new SeriesEntryRow("Fast", 2, R2, 2, 2.0, null, null)
            });
            _repo.GetBoats(Arg.Any<IReadOnlyCollection<int>>()).Returns(new Dictionary<int, BoatDisplayInfo>
            {
                [1] = new BoatDisplayInfo("Alpha", "Laser", "111"),
                [2] = new BoatDisplayInfo("Bravo", "Solo", "222")
            });

            var vm = Build(5);

            Assert.Equal("Spring", vm.SeriesName);
            var display = Assert.Single(vm.Displays);
            Assert.Equal("Fast", display.ClassName);
            Assert.Equal("Spring - Fast", display.SeriesName);
            Assert.Equal(2, display.Entries);
            Assert.Equal(2, display.RaceColumnCount);

            var alpha = display.Rows.Single(r => r.Boatname == "Alpha");
            var bravo = display.Rows.Single(r => r.Boatname == "Bravo");
            Assert.Equal(1, alpha.Place);
            Assert.Equal(2.0, alpha.Score);
            Assert.Equal(2, alpha.Cells.Count);
            Assert.Equal(2, bravo.Place);
            Assert.Equal(4.0, bravo.Score);

            _repo.Received(1).SaveSeriesResults(5, "Fast",
                Arg.Is<IReadOnlyList<SeriesResultRow>>(rows => rows.Count == 2));
        }

        [Fact]
        public void Build_SplitsByClass_IntoSeparateDisplays()
        {
            _repo.GetSeriesHeader(5).Returns(new SeriesResultHeader("Spring", "0,0"));
            _repo.GetRacesToScore(5).Returns(new List<SeriesRaceToScore>());
            _repo.GetEntryRows(5).Returns(new List<SeriesEntryRow>
            {
                new SeriesEntryRow("Fast", 1, R1, 1, 1.0, null, null),
                new SeriesEntryRow("Slow", 2, R2, 3, 1.0, null, null)
            });
            _repo.GetBoats(Arg.Any<IReadOnlyCollection<int>>())
                .Returns(new Dictionary<int, BoatDisplayInfo>());

            var vm = Build(5);

            Assert.Equal(2, vm.Displays.Count);
            Assert.Contains(vm.Displays, d => d.ClassName == "Fast");
            Assert.Contains(vm.Displays, d => d.ClassName == "Slow");
            _repo.Received(1).SaveSeriesResults(5, "Fast", Arg.Any<IReadOnlyList<SeriesResultRow>>());
            _repo.Received(1).SaveSeriesResults(5, "Slow", Arg.Any<IReadOnlyList<SeriesResultRow>>());
        }

        [Fact]
        public void Build_PrunesClassWithNoFinishers()
        {
            // Only coded (non-finishing) rows -> the event has zero finishers and is dropped, leaving
            // the class empty, so no display is produced and nothing is saved.
            _repo.GetSeriesHeader(5).Returns(new SeriesResultHeader("Spring", "0,0"));
            _repo.GetRacesToScore(5).Returns(new List<SeriesRaceToScore>());
            _repo.GetEntryRows(5).Returns(new List<SeriesEntryRow>
            {
                new SeriesEntryRow("Fast", 1, R1, 1, null, null, "DNF")
            });
            _repo.GetBoats(Arg.Any<IReadOnlyCollection<int>>())
                .Returns(new Dictionary<int, BoatDisplayInfo>());

            var vm = Build(5);

            Assert.Empty(vm.Displays);
            _repo.DidNotReceive().SaveSeriesResults(Arg.Any<int>(), Arg.Any<string>(),
                Arg.Any<IReadOnlyList<SeriesResultRow>>());
        }
    }
}
