using System;
using System.Collections;
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

        public BoatSelectRule(Guid? id)
            : this()
        {
            var c = new Db(@"SELECT [name], [application]
                FROM [select_rules]
                WHERE [id] = @id");
            var p = new Hashtable();
            p["id"] = id;
            Hashtable d = c.GetHashtable(p);
            Name = d["name"] as string;
            Rule = RuleType.Compound;
            Application = (d["application"] != null)
                ? (Apply?) Enum.GetValues(typeof (Apply)).GetValue((int) d["application"])
                : null;
            Id = id;
            AddChildren(this);
        }

        public BoatSelectRule(string name)
            : this()
        {
            var c = new Db(@"SELECT [id], [application]
                FROM [select_rules]
                WHERE [name] = @name");
            var p = new Hashtable();
            p["name"] = name;
            Hashtable d = c.GetHashtable(p);
            Id = d["id"] as Guid?;
            Rule = RuleType.Compound;
            Application = (d["application"] != null)
                ? (Apply?) Enum.GetValues(typeof (Apply)).GetValue((int) d["application"])
                : null;
            Name = name;
            AddChildren(this);
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

        public void Delete()
        {
            var del = new Db("DELETE FROM select_rules " +
                             "WHERE id = @id");
            var d = new Hashtable();
            d["id"] = Id;
            del.ExecuteNonQuery(d);
            foreach (BoatSelectRule b in Children)
                b.Delete();
            foreach (BoatSelectRule b in DeletedChildren)
                b.Delete();
        }

        private void AddChildren(BoatSelectRule parent)
        {
            var c = new Db(@"SELECT [id], [application], [field], [condition],
                    [string_value], [number_bound1], [number_bound2]
                    FROM [select_rules]
                    WHERE [parent] = @parent");
            var p = new Hashtable();
            p["parent"] = parent.Id;
            DataTable d = c.GetData(p);
            foreach (DataRow r in d.Rows)
            {
                var child = new BoatSelectRule {Id = r["id"] as Guid?};
                if (r["field"] != DBNull.Value)
                {
                    child.Field = (from n in Fields
                        where n.Name.Equals(r["field"] as string)
                        select n).First<Field>();
                    child.Condition =
                        (ConditionType) Enum.GetValues(typeof (ConditionType)).GetValue((int) r["condition"]);
                    child.Rule = RuleType.Simple;
                    child.StringValue = r["string_value"] as string;
                    child.Bound1 = r["number_bound1"] as decimal?;
                    child.Bound2 = r["number_bound2"] as decimal?;
                }
                else
                {
                    child.Rule = RuleType.Compound;
                    try
                    {
                        child.Application = (Apply) Enum.GetValues(typeof (Apply)).GetValue((int) r["application"]);
                    }
                    catch
                    {
                        child.Application = Apply.Any;
                    }
                    AddChildren(child);
                }
                parent.Add(child);
            }
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

        //
        // Add rule to database.
        //
        public void Save()
        {
            Db c;
            var p = new Hashtable();
            if (Id.HasValue)
            {
                c = new Db(@"UPDATE select_rules
                    SET name = @name
                    , parent = @parent
                    , application = @application
                    , field = @field
                    , condition = @condition
                    , string_value = @string_value
                    , number_bound1 = @number_bound1
                    , number_bound2 = @number_bound2
                    WHERE id = @id");
                p["id"] = Id;
            }
            else
            {
                c = new Db(@"INSERT INTO select_rules
                    (id, name, parent, application, field, condition, string_value, number_bound1, number_bound2)
                    VALUES (@id, @name, @parent, @application, @field, @condition, @string_value, @number_bound1, @number_bound2)");
                Id = Guid.NewGuid();
                p["id"] = Id;
            }
            p["name"] = Name;
            p["parent"] = (Parent == null) ? null : Parent.Id;
            p["application"] = Application;
            p["field"] = (Field == null) ? null : Field.Name;
            p["condition"] = Condition;
            p["string_value"] = StringValue;
            p["number_bound1"] = Bound1;
            p["number_bound2"] = Bound2;
            c.ExecuteNonQuery(p);

            foreach (BoatSelectRule child in Children)
                child.Save();

            foreach (var child in _deletedChildren)
                child.Delete();
        }
    }
}