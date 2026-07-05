using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OodHelper.Rules
{
    /// <summary>
    /// View-model wrapper over a single <see cref="BoatSelectRule"/> node for the rule-editor
    /// TreeView. Add/remove of siblings is driven by commands carrying the node itself, so no
    /// visual-tree traversal is needed.
    /// </summary>
    public partial class SelectRuleNodeViewModel : ObservableObject
    {
        private readonly BoatSelectRule _rule;

        public SelectRuleNodeViewModel(BoatSelectRule rule)
            : this(rule, null)
        {
        }

        public SelectRuleNodeViewModel(BoatSelectRule rule, SelectRuleNodeViewModel parent)
        {
            _rule = rule;
            Parent = parent;
            Children = new ObservableCollection<SelectRuleNodeViewModel>(
                rule.Children.Select(child => new SelectRuleNodeViewModel(child, this)));
        }

        public BoatSelectRule Rule => _rule;

        public SelectRuleNodeViewModel Parent { get; }

        public ObservableCollection<SelectRuleNodeViewModel> Children { get; }

        [RelayCommand]
        private void AddSibling()
        {
            if (Parent == null) return;

            var b = new BoatSelectRule();
            _rule.Parent.Add(b);
            InsertSibling(new SelectRuleNodeViewModel(b, Parent));
        }

        [RelayCommand]
        private void AddParentSibling()
        {
            if (Parent == null) return;

            var b = new BoatSelectRule();
            _rule.Parent.Add(b);
            b.Add(new BoatSelectRule());
            InsertSibling(new SelectRuleNodeViewModel(b, Parent));
        }

        [RelayCommand]
        private void RemoveMe()
        {
            if (Parent == null) return;

            Parent.Children.Remove(this);
            _rule.RemoveFromParent();
            Parent.NotifyStructureChanged();
        }

        private void InsertSibling(SelectRuleNodeViewModel sibling)
        {
            int index = Parent.Children.IndexOf(this);
            Parent.Children.Insert(index + 1, sibling);
            Parent.NotifyStructureChanged();
        }

        //
        // After a child is added or removed, the computed visibilities of this node and its
        // children no longer match the collection; refresh them. (The TreeView itself updates
        // automatically because Children is an ObservableCollection.)
        //
        private void NotifyStructureChanged()
        {
            OnPropertyChanged(nameof(HasChildren));
            OnPropertyChanged(nameof(HasNoChildren));
            foreach (var child in Children)
                child.OnPropertyChanged(nameof(RemoveMeVisibility));
        }

        public bool IsExpanded { get; set; } = true;

        public bool HasChildren => Children.Count > 0;
        public bool HasNoChildren => Children.Count == 0;

        public Visibility ButtonVisibility =>
            Parent == null ? Visibility.Collapsed : Visibility.Visible;

        public Visibility RemoveMeVisibility =>
            Parent != null && Parent.Children.Count == 1 ? Visibility.Collapsed : Visibility.Visible;

        public Apply? Application
        {
            get => _rule.Application;
            set
            {
                _rule.Application = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ConditionType> Conditions
        {
            get
            {
                if (_rule.Field != null)
                    return _rule.Field.conditions;
                return Enum.GetValues(typeof(ConditionType)) as IEnumerable<ConditionType>;
            }
        }

        public Field Field
        {
            get => _rule.Field;
            set
            {
                _rule.Field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Conditions));
                OnPropertyChanged(nameof(Bound1Visible));
                OnPropertyChanged(nameof(Bound2Visible));
                OnPropertyChanged(nameof(StringValueVisible));
            }
        }

        public ConditionType Condition
        {
            get => _rule.Condition;
            set
            {
                _rule.Condition = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Bound1Visible));
                OnPropertyChanged(nameof(Bound2Visible));
                OnPropertyChanged(nameof(StringValueVisible));
            }
        }

        public decimal? Bound1
        {
            get => _rule.Bound1;
            set { _rule.Bound1 = value; OnPropertyChanged(); }
        }

        public decimal? Bound2
        {
            get => _rule.Bound2;
            set { _rule.Bound2 = value; OnPropertyChanged(); }
        }

        public string StringValue
        {
            get => _rule.StringValue;
            set { _rule.StringValue = value; OnPropertyChanged(); }
        }

        public Visibility StringValueVisible
        {
            get
            {
                if (_rule.Field == null) return Visibility.Collapsed;
                return _rule.Field.FieldType == typeof(string) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility Bound1Visible
        {
            get
            {
                if (_rule.Field == null || _rule.Field.FieldType != typeof(int))
                    return Visibility.Collapsed;
                switch (Condition)
                {
                    case ConditionType.False:
                    case ConditionType.True:
                        return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public Visibility Bound2Visible
        {
            get
            {
                if (_rule.Field == null || _rule.Field.FieldType != typeof(int))
                    return Visibility.Collapsed;
                return Condition == ConditionType.Between ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public IEnumerable<Field> EntityFields => BoatSelectRule.Fields;
    }
}
