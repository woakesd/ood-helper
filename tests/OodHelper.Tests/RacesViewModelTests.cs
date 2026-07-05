using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Services;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class RacesViewModelTests
    {
        private readonly ICalendarRepository _repo = Substitute.For<ICalendarRepository>();
        private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

        private RacesViewModel CreateViewModel()
        {
            return new RacesViewModel(_repo, _dialogs) { DebounceMilliseconds = 0 };
        }

        private static IReadOnlyList<Calendar> Items(params (int Rid, string Event)[] items)
        {
            return items.Select(i => new Calendar { Rid = i.Rid, Event = i.Event }).ToList();
        }

        [Fact]
        public void Load_PopulatesRowsFromRepository()
        {
            _repo.GetAll(string.Empty).Returns(Items((1, "Spring 1"), (2, "Spring 2")));
            var vm = CreateViewModel();

            vm.Load();

            Assert.NotNull(vm.Rows);
            Assert.Equal(2, vm.Rows.Count);
        }

        [Fact]
        public async Task SettingFilterText_FiltersRepository()
        {
            _repo.GetAll("spr").Returns(Items((1, "Spring 1")));
            var vm = CreateViewModel();

            vm.FilterText = "spr";
            await vm.FilterTask;

            Assert.NotNull(vm.Rows);
            Assert.Single(vm.Rows);
            _repo.Received(1).GetAll("spr");
        }

        [Fact]
        public void Add_WhenSaved_Reloads()
        {
            _dialogs.ShowRaceEditor(0).Returns(true);
            _repo.GetAll(string.Empty).Returns(Items((1, "Spring 1")));
            var vm = CreateViewModel();

            vm.AddCommand.Execute(null);

            _dialogs.Received(1).ShowRaceEditor(0);
            Assert.NotNull(vm.Rows);
            Assert.Single(vm.Rows);
        }

        [Fact]
        public void Add_WhenCancelled_DoesNotReload()
        {
            _dialogs.ShowRaceEditor(0).Returns(false);
            var vm = CreateViewModel();

            vm.AddCommand.Execute(null);

            Assert.Null(vm.Rows);
            _repo.DidNotReceive().GetAll(Arg.Any<string>());
        }

        [Fact]
        public void Edit_WithSelection_OpensEditorForThatRace()
        {
            _dialogs.ShowRaceEditor(7).Returns(true);
            _repo.GetAll(string.Empty).Returns(Items((7, "Spring 1")));
            var vm = CreateViewModel();
            vm.SelectedRow = new Calendar { Rid = 7, Event = "Spring 1" };

            vm.EditCommand.Execute(null);

            _dialogs.Received(1).ShowRaceEditor(7);
            Assert.NotNull(vm.Rows);
        }

        [Fact]
        public void Edit_WithoutSelection_DoesNothing()
        {
            var vm = CreateViewModel();

            vm.EditCommand.Execute(null);

            _dialogs.DidNotReceive().ShowRaceEditor(Arg.Any<int>());
        }

        [Fact]
        public void Delete_ConfirmsPerRow_AndHonoursYesNoCancel()
        {
            var a = new Calendar { Rid = 1, Event = "Alpha" };
            var b = new Calendar { Rid = 2, Event = "Bravo" };
            var c = new Calendar { Rid = 3, Event = "Charlie" };
            _repo.GetAll(string.Empty).Returns(Items());
            var vm = CreateViewModel();
            var selected = new List<Calendar> { a, b, c };

            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Alpha")), Arg.Any<string>()).Returns(true);
            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Bravo")), Arg.Any<string>()).Returns(false);
            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Charlie")), Arg.Any<string>()).Returns((bool?)null);

            vm.DeleteCommand.Execute(selected);

            _repo.Received(1).Delete(a.Rid);
            _repo.DidNotReceive().Delete(b.Rid);
            _repo.DidNotReceive().Delete(c.Rid);
            _repo.Received(1).GetAll(string.Empty);
        }

        [Fact]
        public void Delete_WithNoSelection_DoesNothing()
        {
            var vm = CreateViewModel();

            vm.DeleteCommand.Execute(new List<Calendar>());

            _dialogs.DidNotReceive().ConfirmYesNoCancel(Arg.Any<string>(), Arg.Any<string>());
            _repo.DidNotReceive().Delete(Arg.Any<int>());
        }
    }
}
