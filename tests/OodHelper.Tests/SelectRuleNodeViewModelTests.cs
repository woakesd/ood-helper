using System.Linq;
using OodHelper.Rules;
using Xunit;

namespace OodHelper.Tests
{
    public class SelectRuleNodeViewModelTests
    {
        private static (SelectRuleNodeViewModel root, BoatSelectRule rootRule) BuildRoot()
        {
            var rootRule = new BoatSelectRule { Rule = RuleType.Compound, Application = Apply.Any };
            rootRule.Add(new BoatSelectRule { Condition = ConditionType.Equals });
            return (new SelectRuleNodeViewModel(rootRule), rootRule);
        }

        [Fact]
        public void AddSibling_AddsToBothTreeAndViewModel()
        {
            var (root, rootRule) = BuildRoot();
            var child = root.Children[0];

            child.AddSiblingCommand.Execute(null);

            Assert.Equal(2, root.Children.Count);
            Assert.Equal(2, rootRule.Children.Count);
        }

        [Fact]
        public void AddParentSibling_AddsCompoundSiblingWithChild()
        {
            var (root, rootRule) = BuildRoot();
            var child = root.Children[0];

            child.AddParentSiblingCommand.Execute(null);

            Assert.Equal(2, root.Children.Count);
            var added = root.Children[1];
            Assert.Single(added.Children);
            Assert.Single(rootRule.Children.Last().Children);
        }

        [Fact]
        public void RemoveMe_RemovesFromTreeAndRecordsDeletion()
        {
            var (root, rootRule) = BuildRoot();
            var first = root.Children[0];
            first.AddSiblingCommand.Execute(null); // now two children
            var second = root.Children[1];

            second.RemoveMeCommand.Execute(null);

            Assert.Single(root.Children);
            Assert.Single(rootRule.Children);
            Assert.Contains(second.Rule, rootRule.DeletedChildren);
        }

        [Fact]
        public void Root_HasNoActionButtons()
        {
            var (root, _) = BuildRoot();

            Assert.Equal(System.Windows.Visibility.Collapsed, root.ButtonVisibility);
        }
    }
}
