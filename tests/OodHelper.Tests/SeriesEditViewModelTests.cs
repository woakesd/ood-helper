using System.Collections.Generic;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Services;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class SeriesEditViewModelTests
    {
        private readonly ISeriesRepository _repo = Substitute.For<ISeriesRepository>();
        private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

        [Fact]
        public void NewSeries_IsEmptyAndDoesNotLoad()
        {
            var vm = new SeriesEditViewModel(_repo, _dialogs, 0);

            Assert.Null(vm.Sname);
            Assert.Empty(vm.Calendar);
            _repo.DidNotReceive().Get(Arg.Any<int>());
            _repo.DidNotReceive().GetMemberRaces(Arg.Any<int>());
        }

        [Fact]
        public void ExistingSeries_LoadsNameDiscardsAndMemberRaces()
        {
            _repo.Get(5).Returns(new Series { Sid = 5, Sname = "Spring", Discards = "0,1" });
            _repo.GetMemberRaces(5).Returns(new List<SeriesRaceItem>
            {
                new SeriesRaceItem(10, "Race A", "Fast", null),
                new SeriesRaceItem(11, "Race B", "Slow", null)
            });

            var vm = new SeriesEditViewModel(_repo, _dialogs, 5);

            Assert.Equal("Spring", vm.Sname);
            Assert.Equal("0,1", vm.Discards);
            Assert.Equal(2, vm.Calendar.Count);
        }

        [Fact]
        public void Save_BlankName_ShowsErrorAndDoesNotSaveOrClose()
        {
            var vm = new SeriesEditViewModel(_repo, _dialogs, 0) { Sname = "   " };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SaveCommand.Execute(null);

            _dialogs.Received(1).ShowError(Arg.Any<string>(), Arg.Any<string>());
            _repo.DidNotReceive().Save(Arg.Any<Series>());
            Assert.Null(closed);
        }

        [Fact]
        public void Save_Valid_PersistsAndRequestsClose()
        {
            Series saved = null;
            _repo.Save(Arg.Any<Series>()).Returns(42).AndDoes(ci => saved = ci.Arg<Series>());
            var vm = new SeriesEditViewModel(_repo, _dialogs, 0) { Sname = "Winter", Discards = "0,1" };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SaveCommand.Execute(null);

            Assert.NotNull(saved);
            Assert.Equal("Winter", saved.Sname);
            Assert.Equal(42, vm.Sid);
            Assert.True(closed);
        }

        [Fact]
        public void Cancel_RequestsCloseWithoutSaving()
        {
            var vm = new SeriesEditViewModel(_repo, _dialogs, 0);
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.CancelCommand.Execute(null);

            Assert.False(closed);
            _repo.DidNotReceive().Save(Arg.Any<Series>());
        }

        [Fact]
        public void EditRaces_NewSeries_ShowsErrorAndDoesNotOpenPicker()
        {
            var vm = new SeriesEditViewModel(_repo, _dialogs, 0);

            vm.EditRacesCommand.Execute(null);

            _dialogs.Received(1).ShowError(Arg.Any<string>(), Arg.Any<string>());
            _dialogs.DidNotReceive().ShowSeriesRaceSelect(Arg.Any<int>());
        }

        [Fact]
        public void EditRaces_ExistingSeries_OpensPickerAndReloadsOnAccept()
        {
            _repo.Get(5).Returns(new Series { Sid = 5, Sname = "Spring" });
            _repo.GetMemberRaces(5).Returns(new List<SeriesRaceItem>());
            _dialogs.ShowSeriesRaceSelect(5).Returns(true);
            var vm = new SeriesEditViewModel(_repo, _dialogs, 5);

            vm.EditRacesCommand.Execute(null);

            _dialogs.Received(1).ShowSeriesRaceSelect(5);
            // Once in the constructor, once after the picker is accepted.
            _repo.Received(2).GetMemberRaces(5);
        }
    }
}
