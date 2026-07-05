using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace OodHelper.Rules
{
    public enum FieldType
    {
        FtInt,
        FtString,
        FtBoolean
    }

    public enum RuleType
    {
        Simple,
        Compound
    }

    public enum Apply
    {
        Any,
        All
    }

    public enum ConditionType
    {
        Equals,
        NotEqual,
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        Contains,
        StartWith,
        EndsWith,
        Between,
        True,
        False
    }

    public enum EventType
    {
        MembersOnly,
        Open
    }

    public class BoatSelectRule
    {
        public static List<Field> Fields = new List<Field>
        {
            new Field("Boat class", "boatclass", typeof (string)),
            new Field("Dinghy", "dinghy", typeof (bool)),
            new Field("Open handicap", "open_handicap", typeof (int)),
            new Field("Hull Type", "hulltype", typeof (string))
        };

        private readonly List<BoatSelectRule> _children = new List<BoatSelectRule>();
        private readonly List<BoatSelectRule> _deletedChildren = new List<BoatSelectRule>();

        public BoatSelectRule()
        {
        }

        public BoatSelectRule Parent { get; set; }

        public Guid? Id { get; set; }
        public string Name { get; set; }
        public RuleType Rule { get; set; }
        //
        // For simple rules the following will be used.
        //
        public Field Field { get; set; }
        public ConditionType Condition { get; set; }
        public string StringValue { get; set; }
        public decimal? Bound1 { get; set; }
        public decimal? Bound2 { get; set; }

        //
        // For a compound rule only these will be used.
        //
        public Apply? Application { get; set; }

        public ReadOnlyCollection<BoatSelectRule> Children
        {
            get { return new ReadOnlyCollection<BoatSelectRule>(_children); }
        }

        public ReadOnlyCollection<BoatSelectRule> DeletedChildren
        {
            get { return new ReadOnlyCollection<BoatSelectRule>(_deletedChildren); }
        }

        public bool AppliesToBoat(DataRowView boat)
        {
            switch (Rule)
            {
                case RuleType.Compound:
                    switch (Application)
                    {
                        case Apply.All:
                            return Children.All(c => c.AppliesToBoat(boat));
                        case Apply.Any:
                            return Children.Any(c => c.AppliesToBoat(boat));
                    }
                    break;
                case RuleType.Simple:
                    object val = boat[Field.Column];
                    if (val != DBNull.Value)
                    {
                        switch (Condition)
                        {
                            case ConditionType.GreaterThan:
                                if ((int) val > Bound1)
                                    return true;
                                break;
                            case ConditionType.GreaterThanOrEqualTo:
                                if ((int) val >= Bound1)
                                    return true;
                                break;
                            case ConditionType.Between:
                                if ((int) val >= Bound1 && (int) val <= Bound2)
                                    return true;
                                break;
                            case ConditionType.LessThan:
                                if ((int) val < Bound1)
                                    return true;
                                break;
                            case ConditionType.LessThanOrEqualTo:
                                if ((int) val <= Bound1)
                                    return true;
                                break;
                            case ConditionType.Contains:
                                if (((string) val).ToLower().Contains(StringValue.ToLower()))
                                    return true;
                                break;
                            case ConditionType.EndsWith:
                                if (((string) val).ToLower().EndsWith(StringValue.ToLower()))
                                    return true;
                                break;
                            case ConditionType.StartWith:
                                if (((string) val).ToLower().StartsWith(StringValue.ToLower()))
                                    return true;
                                break;
                            case ConditionType.Equals:
                                if (((string) val).ToLower().Equals(StringValue.ToLower()))
                                    return true;
                                break;
                            case ConditionType.NotEqual:
                                if (!((string) val).ToLower().Equals(StringValue.ToLower()))
                                    return true;
                                break;
                            case ConditionType.False:
                                if (!(bool) val)
                                    return true;
                                break;
                            case ConditionType.True:
                                if ((bool) val)
                                    return true;
                                break;
                        }
                    }
                    break;
            }
            return false;
        }

        public void Add(BoatSelectRule bsr)
        {
            bsr.Parent = this;
            _children.Add(bsr);
        }

        public void RemoveFromParent()
        {
            Parent._children.Remove(this);
            Parent._deletedChildren.Add(this);
        }
    }
}