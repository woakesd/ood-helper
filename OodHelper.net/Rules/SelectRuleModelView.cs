using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OodHelper.Rules
{
    class SelectRuleModelView : NotifyPropertyChanged
    {
        public SelectRuleModelView(BoatSelectRule rule)
            : this(rule, null)
        {
        }

        public SelectRuleModelView(BoatSelectRule rule, SelectRuleModelView parent)
        {
            _rule = rule;
            Parent = parent;
            _children = (from child in rule.Children
                 select new SelectRuleModelView(child, this)).ToList<SelectRuleModelView>();
        }

        public void Add(SelectRuleModelView srule)
        {
            _children.Add(srule);
            OnPropertyChanged("Children");
            OnPropertyChanged("RemoveMeVisibility");
        }

        public void AddSibling(SelectRuleModelView srule)
        {
            Parent._children.Insert(Parent._children.IndexOf(this)+1, srule);
            Parent.OnPropertyChanged("Children");
            Parent.OnPropertyChanged("RemoveMeVisibility");
        }

        public System.Windows.Visibility ButtonVisibility
        {
            get
            {
                if (Parent == null)
                    return System.Windows.Visibility.Collapsed;
                return System.Windows.Visibility.Visible;
            }
        }

        public BoatSelectRule Rule
        {
            get
            {
                return _rule;
            }
        }

        public void RemoveFromParent()
        {
            Parent._children.Remove(this);
            _rule.RemoveFromParent();
            Parent.OnPropertyChanged("Children");
            OnPropertyChanged("RemoveMeVisibility");
        }

        private bool _isExpanded = true;
        public bool IsExpanded { get { return _isExpanded; } set { _isExpanded = value; } }

        public bool HasChildren { get { return (_children.Count > 0); } }
        public bool HasNoChildren { get { return (_children.Count == 0); } }

        public Apply Application
        {
            get
            {
                return _rule.Application;
            }
            set
            {
                _rule.Application = value;
                OnPropertyChanged("Application");
            }
        }

        public IEnumerable<ConditionType> Conditions
        {
            get
            {
                if (_rule != null && _rule.Field != null)
                    return _rule.Field.conditions;
                return System.Enum.GetValues(typeof(ConditionType)) as IEnumerable<ConditionType>;
            }
        }

        public Field Field
        {
            get { return _rule.Field; }
            set
            {
                _rule.Field = value;
                OnPropertyChanged("Field");
                OnPropertyChanged("Conditions");
                OnPropertyChanged("Bound1Visible");
                OnPropertyChanged("Bound2Visible");
                OnPropertyChanged("StringValueVisible");
            }
        }

        public ConditionType Condition
        {
            get { return _rule.Condition; }
            set
            {
                _rule.Condition = value; 
                OnPropertyChanged("Condition");
                OnPropertyChanged("Bound1Visible");
                OnPropertyChanged("Bound2Visible");
                OnPropertyChanged("StringValueVisible");
            }
        }
        public decimal? Bound1 { get { return _rule.Bound1; } set { _rule.Bound1 = value; OnPropertyChanged("Bound1"); } }
        public decimal? Bound2 { get { return _rule.Bound2; } set { _rule.Bound2 = value; OnPropertyChanged("Bound2"); } }

        public System.Windows.Visibility RemoveMeVisibility
        {
            get
            {
                if (Parent != null && Parent.Children.Count == 1)
                    return System.Windows.Visibility.Collapsed;
                return System.Windows.Visibility.Visible;
            }
        }

        public System.Windows.Visibility StringValueVisible
        {
            get
            {
                if (_rule.Field == null)
                    return System.Windows.Visibility.Collapsed;
                if (_rule.Field.FieldType == typeof(string))
                    return System.Windows.Visibility.Visible;
                return System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility Bound1Visible
        {
            get
            {
                if (_rule.Field == null)
                    return System.Windows.Visibility.Collapsed;
                if (_rule.Field.FieldType != typeof(int))
                    return System.Windows.Visibility.Collapsed;

                switch (Condition)
                {
                    case ConditionType.False:
                    case ConditionType.True:
                        return System.Windows.Visibility.Collapsed;
                        //break;
                }
                return System.Windows.Visibility.Visible;
            }
        }

        public System.Windows.Visibility Bound2Visible
        {
            get
            {
                if (_rule.Field == null)
                    return System.Windows.Visibility.Collapsed;
                if (_rule.Field.FieldType != typeof(int))
                    return System.Windows.Visibility.Collapsed;

                if (Condition == ConditionType.Between)
                    return System.Windows.Visibility.Visible;
                return System.Windows.Visibility.Collapsed;
            }
        }

        BoatSelectRule _rule;
        public SelectRuleModelView Parent { get; private set; }
        private List<SelectRuleModelView> _children;

        public ReadOnlyCollection<SelectRuleModelView> Children
        {
            get
            {
                return new ReadOnlyCollection<SelectRuleModelView>(_children);
            }
        }

        public string StringValue
        {
            get
            {
                return _rule.StringValue;
            }

            set
            {
                _rule.StringValue = value;
            }
        }

        public decimal? NumberBound1
        {
            get
            {
                return _rule.Bound1;
            }
            set
            {
                _rule.Bound1 = value;
            }
        }

        public decimal? NumberBound2
        {
            get
            {
                return _rule.Bound2;
            }
            set
            {
                _rule.Bound2 = value;
            }
        }

        public IEnumerable<Field> EntityFields
        {
            get
            {
                return BoatSelectRule.Fields;
            }
        }

        public string[] ConditionNames
        {
            get
            {
                string[] conditionNames = Enum.GetNames(typeof(ConditionType));
                for (int i = 0; i < conditionNames.Length; i++)
                    conditionNames[i] = conditionNames[i].Replace('_', ' ');
                return conditionNames;
            }
        }
    }
}
