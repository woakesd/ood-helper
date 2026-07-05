using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using OodHelper.Data;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class SeriesRaceSelectViewModelTests
    {
        private readonly ISeriesRepository _repo = Substitute.For<ISeriesRepository>();

        private SeriesRaceSelectViewModel CreateLoaded(int sid = 1)
        {
            _repo.GetAllRacesWithMembership(sid).Returns(new List<SeriesRaceCandidate>
            {
                new SeriesRaceCandidate(10, "Spring Series 1", "Fast", null, true),
                new SeriesRaceCandidate(11, "Spring Series 2", "Slow", null, false),
                new SeriesRaceCandidate(12, "Autumn Series 1", "Fast", null, false)
            });
            var vm = new SeriesRaceSelectViewModel(_repo, sid);
            vm.Load();
            return vm;
        }

        [Fact]
        public void Load_BuildsCandidatesWithMembership()
        {
            var vm = CreateLoaded();

            Assert.Equal(3, vm.Races.Count);
            Assert.True(vm.Races.Single(r => r.Rid == 10).Selected);
            Assert.False(vm.Races.Single(r => r.Rid == 11).Selected);
        }

        [Fact]
        public void FilterText_NarrowsView()
        {
            var vm = CreateLoaded();

            vm.FilterText = "Autumn";

            var visible = vm.RacesView.Cast<SeriesRaceCandidateViewModel>().ToList();
            Assert.Single(visible);
            Assert.Equal(12, visible[0].Rid);
        }

        [Fact]
        public void SelectAll_TicksOnlyFilteredRows()
        {
            var vm = CreateLoaded();
            vm.FilterText = "Autumn";

            vm.SelectAllCommand.Execute(null);

            Assert.True(vm.Races.Single(r => r.Rid == 12).Selected);
            // Row 11 is filtered out, so it stays unticked.
            Assert.False(vm.Races.Single(r => r.Rid == 11).Selected);
        }

        [Fact]
        public void Save_PersistsSelectedRidsAndRequestsClose()
        {
            IEnumerable<int> savedRids = null;
            _repo.When(r => r.SetMemberRaces(1, Arg.Any<IEnumerable<int>>()))
                .Do(ci => savedRids = ci.Arg<IEnumerable<int>>().ToList());
            var vm = CreateLoaded();
            vm.Races.Single(r => r.Rid == 11).Selected = true; // now 10 and 11 selected
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SaveCommand.Execute(null);

            Assert.NotNull(savedRids);
            Assert.Equal(new[] { 10, 11 }, savedRids.OrderBy(x => x).ToArray());
            Assert.True(closed);
        }

        [Fact]
        public void Cancel_RequestsCloseWithoutSaving()
        {
            var vm = CreateLoaded();
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.CancelCommand.Execute(null);

            Assert.False(closed);
            _repo.DidNotReceive().SetMemberRaces(Arg.Any<int>(), Arg.Any<IEnumerable<int>>());
        }
    }
}
