using System.Collections.Generic;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Results;
using OodHelper.Services;
using Xunit;

namespace OodHelper.Tests
{
    public class ResultsEditorViewModelTests
    {
        private const int Rid = 5;
        private readonly IRaceResultsRepository _repo = Substitute.For<IRaceResultsRepository>();
        private readonly IRaceScoreRepository _scoreRepo = Substitute.For<IRaceScoreRepository>();
        private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

        private ResultsEditorViewModel CreateLoaded(params Race[] rows)
        {
            _repo.GetCalendar(Rid).Returns(new Calendar { Rid = Rid });
            _repo.GetRaceRows(Rid).Returns(new List<Race>(rows));
            var vm = new ResultsEditorViewModel(Rid, _repo, _scoreRepo, _dialogs);
            vm.Load();
            return vm;
        }

        [Fact]
        public void Load_PopulatesRowsFromRepository()
        {
            var vm = CreateLoaded(
                new Race { Rid = Rid, Bid = 1 },
                new Race { Rid = Rid, Bid = 2 });

            Assert.NotNull(vm.Rows);
            Assert.Equal(2, vm.Rows.Count);
        }

        [Fact]
        public void SettingCourse_PersistsImmediately()
        {
            var vm = CreateLoaded();

            vm.Course = "Triangle";

            _repo.Received(1).UpdateCourse(Rid, "Triangle");
        }

        [Fact]
        public void EditingARow_PersistsAndEnablesCalculate()
        {
            _repo.GetCalculateEnabled(Rid).Returns(false);
            var vm = CreateLoaded(new Race { Rid = Rid, Bid = 1 });
            Assert.False(vm.CalculateEnabled);

            vm.Rows[0].FinishCode = "DNF";

            _repo.Received(1).UpdateRaceRow(Arg.Any<Race>());
            Assert.True(vm.CalculateEnabled);
        }

        [Fact]
        public void SettingRaceType_PersistsAndEnablesCalculate()
        {
            var vm = CreateLoaded();

            vm.RaceType = CalendarModel.RaceTypes.SternChase;

            _repo.Received(1).UpdateRaceType(Rid, "SternChase");
            Assert.True(vm.CalculateEnabled);
        }

        [Fact]
        public void RaceType_DrivesReadOnlyAndVisibilityFlags()
        {
            var vm = CreateLoaded();

            vm.RaceType = CalendarModel.RaceTypes.SternChase;
            Assert.False(vm.StartReadOnly);
            Assert.False(vm.PlaceReadOnly);
            Assert.True(vm.FinishReadOnly);

            vm.RaceType = CalendarModel.RaceTypes.FixedLength;
            Assert.True(vm.StartReadOnly);
            Assert.True(vm.PlaceReadOnly);
            Assert.False(vm.FinishReadOnly);
        }

        [Fact]
        public void AutoPopulate_CountsAndReloads()
        {
            _repo.CountAutoPopulate(Rid).Returns(3);
            var vm = CreateLoaded();

            Assert.Equal(3, vm.CountAutoPopulateData());

            vm.DoAutoPopulate();

            _repo.Received(1).DoAutoPopulate(Rid);
            // Load on construction + reload after auto-populate.
            _repo.Received(2).GetRaceRows(Rid);
        }
    }
}
