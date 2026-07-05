using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Services;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class SelectRulesViewModelTests
    {
        private readonly ISelectRuleRepository _rules = Substitute.For<ISelectRuleRepository>();
        private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

        private SelectRulesViewModel CreateViewModel()
        {
            return new SelectRulesViewModel(_rules, _dialogs) { DebounceMilliseconds = 0 };
        }

        private static IReadOnlyList<SelectRuleListItem> Items(params (Guid Id, string Name)[] items)
        {
            return items.Select(i => new SelectRuleListItem(i.Id, i.Name)).ToList();
        }

        [Fact]
        public void Load_PopulatesRowsFromTopLevel()
        {
            _rules.GetTopLevel(string.Empty).Returns(Items((Guid.NewGuid(), "Yachts"), (Guid.NewGuid(), "Dinghies")));
            var vm = CreateViewModel();

            vm.Load();

            Assert.NotNull(vm.Rows);
            Assert.Equal(2, vm.Rows.Count);
        }

        [Fact]
        public async Task SettingFilterText_FiltersTopLevel()
        {
            _rules.GetTopLevel("dingh").Returns(Items((Guid.NewGuid(), "Dinghies")));
            var vm = CreateViewModel();

            vm.FilterText = "dingh";
            await vm.FilterTask;

            Assert.NotNull(vm.Rows);
            Assert.Single(vm.Rows);
            _rules.Received(1).GetTopLevel("dingh");
        }

        [Fact]
        public void Add_WhenSaved_Reloads()
        {
            _dialogs.ShowSelectRuleEditor(null).Returns(true);
            _rules.GetTopLevel(string.Empty).Returns(Items((Guid.NewGuid(), "Yachts")));
            var vm = CreateViewModel();

            vm.AddCommand.Execute(null);

            _dialogs.Received(1).ShowSelectRuleEditor(null);
            Assert.NotNull(vm.Rows);
            Assert.Single(vm.Rows);
        }

        [Fact]
        public void Add_WhenCancelled_DoesNotReload()
        {
            _dialogs.ShowSelectRuleEditor(null).Returns(false);
            var vm = CreateViewModel();

            vm.AddCommand.Execute(null);

            Assert.Null(vm.Rows);
            _rules.DidNotReceive().GetTopLevel(Arg.Any<string>());
        }

        [Fact]
        public void Edit_WithSelection_OpensEditorForThatRule()
        {
            var id = Guid.NewGuid();
            _dialogs.ShowSelectRuleEditor(id).Returns(true);
            _rules.GetTopLevel(string.Empty).Returns(Items((id, "Yachts")));
            var vm = CreateViewModel();
            vm.SelectedRow = new SelectRuleListItem(id, "Yachts");

            vm.EditCommand.Execute(null);

            _dialogs.Received(1).ShowSelectRuleEditor(id);
            Assert.NotNull(vm.Rows);
        }

        [Fact]
        public void Edit_WithoutSelection_DoesNothing()
        {
            var vm = CreateViewModel();

            vm.EditCommand.Execute(null);

            _dialogs.DidNotReceive().ShowSelectRuleEditor(Arg.Any<Guid?>());
        }

        [Fact]
        public void Delete_ConfirmsPerRow_AndHonoursYesNoCancel()
        {
            var a = new SelectRuleListItem(Guid.NewGuid(), "Alpha");
            var b = new SelectRuleListItem(Guid.NewGuid(), "Bravo");
            var c = new SelectRuleListItem(Guid.NewGuid(), "Charlie");
            _rules.GetTopLevel(string.Empty).Returns(Items());
            var vm = CreateViewModel();
            var selected = new List<SelectRuleListItem> { a, b, c };

            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Alpha")), Arg.Any<string>()).Returns(true);
            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Bravo")), Arg.Any<string>()).Returns(false);
            _dialogs.ConfirmYesNoCancel(Arg.Is<string>(m => m.Contains("Charlie")), Arg.Any<string>()).Returns((bool?)null);

            vm.DeleteCommand.Execute(selected);

            _rules.Received(1).Delete(a.Id);
            _rules.DidNotReceive().Delete(b.Id);
            _rules.DidNotReceive().Delete(c.Id);
            // A real deletion happened, so the grid reloads once.
            _rules.Received(1).GetTopLevel(string.Empty);
        }

        [Fact]
        public void Delete_WithNoSelection_DoesNothing()
        {
            var vm = CreateViewModel();

            vm.DeleteCommand.Execute(new List<SelectRuleListItem>());

            _dialogs.DidNotReceive().ConfirmYesNoCancel(Arg.Any<string>(), Arg.Any<string>());
            _rules.DidNotReceive().Delete(Arg.Any<Guid>());
        }
    }
}
