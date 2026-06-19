using System;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Rules;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class SelectRuleEditViewModelTests
    {
        private readonly ISelectRuleRepository _repo = Substitute.For<ISelectRuleRepository>();

        [Fact]
        public void NewRule_SeedsRootWithOneChild()
        {
            var vm = new SelectRuleEditViewModel(_repo, null);

            Assert.Single(vm.Rules);
            var root = vm.Rules[0];
            Assert.Single(root.Children);
            _repo.DidNotReceive().GetTree(Arg.Any<Guid>());
        }

        [Fact]
        public void ExistingRule_LoadsTreeAndName()
        {
            var id = Guid.NewGuid();
            _repo.GetTree(id).Returns(new BoatSelectRule { Id = id, Name = "Yachts", Rule = RuleType.Compound });

            var vm = new SelectRuleEditViewModel(_repo, id);

            Assert.Equal("Yachts", vm.RuleName);
            Assert.Single(vm.Rules);
        }

        [Fact]
        public void Save_PersistsRootWithNameAndRequestsClose()
        {
            BoatSelectRule saved = null;
            _repo.When(r => r.Save(Arg.Any<BoatSelectRule>())).Do(ci => saved = ci.Arg<BoatSelectRule>());
            var vm = new SelectRuleEditViewModel(_repo, null) { RuleName = "Cruisers" };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SaveCommand.Execute(null);

            Assert.NotNull(saved);
            Assert.Equal("Cruisers", saved.Name);
            Assert.True(closed);
        }

        [Fact]
        public void Cancel_RequestsCloseWithoutSaving()
        {
            var vm = new SelectRuleEditViewModel(_repo, null);
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.CancelCommand.Execute(null);

            Assert.False(closed);
            _repo.DidNotReceive().Save(Arg.Any<BoatSelectRule>());
        }
    }
}
