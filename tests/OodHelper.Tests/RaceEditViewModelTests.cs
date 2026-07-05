using System;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Services;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class RaceEditViewModelTests
    {
        private readonly ICalendarRepository _repo = Substitute.For<ICalendarRepository>();
        private readonly IPortsmouthNumberRepository _pnRepo = Substitute.For<IPortsmouthNumberRepository>();
        private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

        private RaceEditViewModel New() => new RaceEditViewModel(_repo, _pnRepo, _dialogs, 0);

        [Fact]
        public void NewRace_HasDefaults_AndDoesNotLoad()
        {
            var vm = New();

            Assert.True(vm.IsRace);
            Assert.False(vm.Raced);
            Assert.False(vm.Approved);
            Assert.Equal(string.Empty, vm.Racetype);
            Assert.Equal("F", vm.TimeLimitType);
            Assert.True(vm.IsTimeFixed);
            Assert.True(vm.ShowExtension);
            _repo.DidNotReceive().Get(Arg.Any<int>());
        }

        [Fact]
        public void ExistingRace_LoadsFieldsAndFormatsTimes()
        {
            _repo.Get(5).Returns(new Calendar
            {
                Rid = 5,
                Event = "Spring 1",
                Class = "Laser",
                StartDate = new DateTime(2026, 6, 1, 10, 0, 0),
                TimeLimitFixed = new DateTime(2026, 6, 1, 12, 0, 0),
                Extension = 900,
                TimeLimitType = "F",
                IsRace = true
            });

            var vm = new RaceEditViewModel(_repo, _pnRepo, _dialogs, 5);

            Assert.Equal("Spring 1", vm.Event);
            Assert.Equal("Laser", vm.ClassName);
            Assert.Equal(new DateTime(2026, 6, 1), vm.StartDateDate);
            Assert.Equal("10:00", vm.StartDateTime);
            Assert.Equal("12:00", vm.TimeLimitFixedTime);
            Assert.Equal("0:15", vm.ExtensionText);
            Assert.Equal("F", vm.TimeLimitType);
        }

        [Fact]
        public void Save_RaceWithMissingRequiredFields_ShowsErrorAndDoesNotSaveOrClose()
        {
            var vm = New(); // IsRace true, TimeLimitType F, everything else blank
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SaveCommand.Execute(null);

            _dialogs.Received(1).ShowError(Arg.Any<string>(), Arg.Any<string>());
            _repo.DidNotReceive().Save(Arg.Any<Calendar>());
            Assert.Null(closed);
        }

        [Fact]
        public void Save_ValidFixed_PersistsEntityAndCloses()
        {
            Calendar saved = null;
            _repo.Save(Arg.Any<Calendar>()).Returns(42).AndDoes(ci => saved = ci.Arg<Calendar>());

            var vm = New();
            vm.Event = "Spring 1";
            vm.ClassName = "Laser";
            vm.StartDateDate = new DateTime(2026, 6, 1);
            vm.StartDateTime = "10:00";
            vm.TimeLimitFixedDate = new DateTime(2026, 6, 1);
            vm.TimeLimitFixedTime = "12:00";
            vm.ExtensionText = "0:15";
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SaveCommand.Execute(null);

            Assert.NotNull(saved);
            Assert.Equal("Spring 1", saved.Event);
            Assert.Equal("Laser", saved.Class);
            Assert.Equal(new DateTime(2026, 6, 1, 10, 0, 0), saved.StartDate);
            Assert.Equal("F", saved.TimeLimitType);
            Assert.Equal(new DateTime(2026, 6, 1, 12, 0, 0), saved.TimeLimitFixed);
            Assert.Null(saved.TimeLimitDelta);
            Assert.Equal(900, saved.Extension);
            Assert.Equal(42, vm.Rid);
            Assert.True(closed);
        }

        [Fact]
        public void Save_ValidDelta_PersistsDeltaSecondsAndNoFixed()
        {
            Calendar saved = null;
            _repo.Save(Arg.Any<Calendar>()).Returns(7).AndDoes(ci => saved = ci.Arg<Calendar>());

            var vm = New();
            vm.IsTimeDelta = true;
            vm.Event = "Spring 1";
            vm.ClassName = "Laser";
            vm.StartDateDate = new DateTime(2026, 6, 1);
            vm.StartDateTime = "10:00";
            vm.TimeLimitDeltaText = "1:30";
            vm.ExtensionText = "0:15";

            vm.SaveCommand.Execute(null);

            Assert.NotNull(saved);
            Assert.Equal("D", saved.TimeLimitType);
            Assert.Equal(5400, saved.TimeLimitDelta);
            Assert.Null(saved.TimeLimitFixed);
        }

        [Fact]
        public void Save_NonRace_SkipsBusinessValidation()
        {
            _repo.Save(Arg.Any<Calendar>()).Returns(3);
            var vm = New();
            vm.IsRace = false; // blank event/class/date are allowed when it is not a race
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SaveCommand.Execute(null);

            _dialogs.DidNotReceive().ShowError(Arg.Any<string>(), Arg.Any<string>());
            _repo.Received(1).Save(Arg.Any<Calendar>());
            Assert.True(closed);
        }

        [Fact]
        public void SwitchingToDelta_RecomputesFromFixed_AndTogglesVisibility()
        {
            _repo.Get(5).Returns(new Calendar
            {
                Rid = 5,
                StartDate = new DateTime(2026, 6, 1, 10, 0, 0),
                TimeLimitFixed = new DateTime(2026, 6, 1, 12, 0, 0),
                TimeLimitType = "F"
            });
            var vm = new RaceEditViewModel(_repo, _pnRepo, _dialogs, 5);

            vm.IsTimeDelta = true;

            Assert.Equal("D", vm.TimeLimitType);
            Assert.Equal("2:00", vm.TimeLimitDeltaText);
            Assert.True(vm.ShowDeltaTimeLimit);
            Assert.False(vm.ShowFixedTimeLimit);
        }

        [Fact]
        public void SelectClass_WhenPicked_SetsClassName()
        {
            var id = Guid.NewGuid();
            _dialogs.ShowClassPicker().Returns(id);
            _pnRepo.Get(id).Returns(new PortsmouthNumber { Id = id, ClassName = "Laser" });
            var vm = New();

            vm.SelectClassCommand.Execute(null);

            Assert.Equal("Laser", vm.ClassName);
        }

        [Fact]
        public void SelectClass_WhenCancelled_LeavesClassUnchanged()
        {
            _dialogs.ShowClassPicker().Returns((Guid?)null);
            var vm = New();
            vm.ClassName = "Original";

            vm.SelectClassCommand.Execute(null);

            Assert.Equal("Original", vm.ClassName);
            _pnRepo.DidNotReceive().Get(Arg.Any<Guid>());
        }

        [Fact]
        public void Cancel_RequestsCloseFalse()
        {
            var vm = New();
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.CancelCommand.Execute(null);

            Assert.False(closed);
            _repo.DidNotReceive().Save(Arg.Any<Calendar>());
        }
    }
}
