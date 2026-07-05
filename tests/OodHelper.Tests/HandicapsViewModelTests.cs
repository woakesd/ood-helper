using System;
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
    public class HandicapsViewModelTests
    {
        private readonly IPortsmouthNumberRepository _repo = Substitute.For<IPortsmouthNumberRepository>();
        private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

        private HandicapsViewModel CreateViewModel()
        {
            return new HandicapsViewModel(_repo, _dialogs) { DebounceMilliseconds = 0 };
        }

        private static IReadOnlyList<PortsmouthNumber> Items(params (Guid Id, string Name)[] items)
        {
            return items.Select(i => new PortsmouthNumber { Id = i.Id, ClassName = i.Name }).ToList();
        }

        [Fact]
        public void Load_PopulatesRowsFromRepository()
        {
            _repo.GetAll(string.Empty).Returns(Items((Guid.NewGuid(), "Laser"), (Guid.NewGuid(), "Topper")));
            var vm = CreateViewModel();

            vm.Load();

            Assert.NotNull(vm.Rows);
            Assert.Equal(2, vm.Rows.Count);
        }

        [Fact]
        public async Task SettingFilterText_FiltersRepository()
        {
            _repo.GetAll("las").Returns(Items((Guid.NewGuid(), "Laser")));
            var vm = CreateViewModel();

            vm.FilterText = "las";
            await vm.FilterTask;

            Assert.NotNull(vm.Rows);
            Assert.Single(vm.Rows);
            _repo.Received(1).GetAll("las");
        }

        [Fact]
        public void Add_WhenSaved_Reloads()
        {
            _dialogs.ShowHandicapEditor(null).Returns(true);
            _repo.GetAll(string.Empty).Returns(Items((Guid.NewGuid(), "Laser")));
            var vm = CreateViewModel();

            vm.AddCommand.Execute(null);

            _dialogs.Received(1).ShowHandicapEditor(null);
            Assert.NotNull(vm.Rows);
            Assert.Single(vm.Rows);
        }

        [Fact]
        public void Add_WhenCancelled_DoesNotReload()
        {
            _dialogs.ShowHandicapEditor(null).Returns(false);
            var vm = CreateViewModel();

            vm.AddCommand.Execute(null);

            Assert.Null(vm.Rows);
            _repo.DidNotReceive().GetAll(Arg.Any<string>());
        }

        [Fact]
        public void Edit_WithSelection_OpensEditorForThatClass()
        {
            var id = Guid.NewGuid();
            _dialogs.ShowHandicapEditor(id).Returns(true);
            _repo.GetAll(string.Empty).Returns(Items((id, "Laser")));
            var vm = CreateViewModel();
            vm.SelectedRow = new PortsmouthNumber { Id = id, ClassName = "Laser" };

            vm.EditCommand.Execute(null);

            _dialogs.Received(1).ShowHandicapEditor(id);
            Assert.NotNull(vm.Rows);
        }

        [Fact]
        public void Edit_WithoutSelection_DoesNothing()
        {
            var vm = CreateViewModel();

            vm.EditCommand.Execute(null);

            _dialogs.DidNotReceive().ShowHandicapEditor(Arg.Any<Guid?>());
        }

        [Fact]
        public void Delete_ConfirmsPerRow_AndHonoursYesNoCancel()
        {
            var a = new PortsmouthNumber { Id = Guid.NewGuid(), ClassName = "Alpha" };
            var b = new PortsmouthNumber { Id = Guid.NewGuid(), ClassName = "Bravo" };
            var c = new PortsmouthNumber { Id = Guid.NewGuid(), ClassName = "Charlie" };
            _repo.GetAll(string.Empty).Returns(Items());
            var vm = CreateViewModel();
            var selected = new List<PortsmouthNumber> { a, b, c };

            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Alpha")), Arg.Any<string>()).Returns(true);
            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Bravo")), Arg.Any<string>()).Returns(false);
            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Charlie")), Arg.Any<string>()).Returns((bool?)null);

            vm.DeleteCommand.Execute(selected);

            _repo.Received(1).Delete(a.Id);
            _repo.DidNotReceive().Delete(b.Id);
            _repo.DidNotReceive().Delete(c.Id);
            // A real deletion happened, so the grid reloads once.
            _repo.Received(1).GetAll(string.Empty);
        }

        [Fact]
        public void Delete_WithNoSelection_DoesNothing()
        {
            var vm = CreateViewModel();

            vm.DeleteCommand.Execute(new List<PortsmouthNumber>());

            _dialogs.DidNotReceive().ConfirmYesNoCancel(Arg.Any<string>(), Arg.Any<string>());
            _repo.DidNotReceive().Delete(Arg.Any<Guid>());
        }
    }
}
