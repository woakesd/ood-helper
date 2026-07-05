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
    public class BoatsViewModelTests
    {
        private readonly IBoatRepository _boats = Substitute.For<IBoatRepository>();
        private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

        private BoatsViewModel CreateViewModel()
        {
            return new BoatsViewModel(_boats, _dialogs) { DebounceMilliseconds = 0 };
        }

        private static IReadOnlyList<Boat> Boats(params (int Bid, string Name)[] boats)
        {
            return boats.Select(b => new Boat { Bid = b.Bid, Boatname = b.Name }).ToList();
        }

        [Fact]
        public async Task SettingFilterText_SearchesAndPopulatesRows()
        {
            _boats.Search("fire").Returns(Boats((1, "Firefly")));
            var vm = CreateViewModel();

            vm.FilterText = "fire";
            await vm.FilterTask;

            Assert.NotNull(vm.Rows);
            Assert.Single(vm.Rows);
            _boats.Received(1).Search("fire");
        }

        [Fact]
        public async Task SettingFilterTextToWhitespace_ClearsRows()
        {
            _boats.Search("fire").Returns(Boats((1, "Firefly")));
            var vm = CreateViewModel();
            vm.FilterText = "fire";
            await vm.FilterTask;

            vm.FilterText = "   ";
            await vm.FilterTask;

            Assert.Null(vm.Rows);
            _boats.Received(1).Search(Arg.Any<string>());
        }

        [Fact]
        public void Add_WhenAccepted_RefiltersByNewBoatName()
        {
            _dialogs.ShowBoatEditor(0).Returns(new BoatEditResult { Accepted = true, BoatName = "Osprey" });
            _boats.Search("Osprey").Returns(Boats((7, "Osprey")));
            var vm = CreateViewModel();

            vm.AddCommand.Execute(null);

            Assert.Equal("Osprey", vm.FilterText);
            Assert.Single(vm.Rows);
        }

        [Fact]
        public void Add_WhenCancelled_DoesNothing()
        {
            _dialogs.ShowBoatEditor(0).Returns(new BoatEditResult { Accepted = false });
            var vm = CreateViewModel();

            vm.AddCommand.Execute(null);

            Assert.Null(vm.FilterText);
            Assert.Null(vm.Rows);
            _boats.DidNotReceive().Search(Arg.Any<string>());
        }

        [Fact]
        public async Task Edit_WithSelectedRow_OpensEditorForThatBoat()
        {
            _boats.Search("fire").Returns(Boats((42, "Firefly")));
            _dialogs.ShowBoatEditor(42).Returns(new BoatEditResult { Accepted = true, BoatName = "Firefly" });
            var vm = CreateViewModel();
            vm.FilterText = "fire";
            await vm.FilterTask;
            vm.SelectedRow = vm.Rows[0];

            vm.EditCommand.Execute(null);

            _dialogs.Received(1).ShowBoatEditor(42);
            _boats.Received(2).Search("fire");
        }

        [Fact]
        public void Edit_WithoutSelection_DoesNothing()
        {
            var vm = CreateViewModel();

            vm.EditCommand.Execute(null);

            _dialogs.DidNotReceive().ShowBoatEditor(Arg.Any<int>());
        }

        [Fact]
        public async Task Delete_ConfirmsPerRow_AndHonoursYesNoCancel()
        {
            _boats.Search("b").Returns(Boats((1, "Alpha"), (2, "Bravo"), (3, "Charlie")));
            var vm = CreateViewModel();
            vm.FilterText = "b";
            await vm.FilterTask;
            var selected = new List<Boat> { vm.Rows[0], vm.Rows[1], vm.Rows[2] };

            // Yes for Alpha, No for Bravo, Cancel at Charlie
            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Alpha")), Arg.Any<string>()).Returns(true);
            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Bravo")), Arg.Any<string>()).Returns(false);
            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Charlie")), Arg.Any<string>()).Returns((bool?)null);

            vm.DeleteCommand.Execute(selected);

            _boats.Received(1).Delete(1);
            _boats.DidNotReceive().Delete(2);
            _boats.DidNotReceive().Delete(3);
            // One deletion happened, so the grid is reloaded
            _boats.Received(2).Search("b");
        }

        [Fact]
        public void Delete_WithNoSelection_DoesNothing()
        {
            var vm = CreateViewModel();

            vm.DeleteCommand.Execute(new List<Boat>());

            _dialogs.DidNotReceive().ConfirmYesNoCancel(Arg.Any<string>(), Arg.Any<string>());
            _boats.DidNotReceive().Delete(Arg.Any<int>());
        }
    }
}
