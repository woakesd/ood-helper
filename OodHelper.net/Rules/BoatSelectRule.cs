using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;

namespace OodHelper.net.Rules
{
    public enum FieldType { ftInt, ftString, ftBoolean }
    public enum RuleType { Simple, Compound }
    public enum Apply { Any, All }
    public enum ConditionType
    {
        Equals, Not_Equal, 
        Less_Than, Less_Than_Or_Equal_To, 
        Greater_Than, Greater_Than_Or_Equal_To, 
        Contains, Start_With, Ends_With, 
        Between, True, False
    }
    public enum EventType { MembersOnly, Open }

    class BoatSelectRule
    {
        public BoatSelectRule()
        {
        }

        public BoatSelectRule(Guid? id) : this()
        {
            Db c = new Db(@"SELECT [name], [application]
                FROM [select_rules]
                WHERE [id] = @id");
            Hashtable p = new Hashtable();
            p["id"] = id;
            Hashtable d = c.GetHashtable(p);
            Name = d["name"] as string;
            Application = (Apply)d["application"];
            Id = id;
            AddChildren(this);
        }

        public void Delete()
        {
            Db del = new Db("DELETE FROM select_rules " +
                "WHERE id = @id");
            Hashtable d = new Hashtable();
            d["id"] = Id;
            del.ExecuteNonQuery(d);
            foreach (BoatSelectRule b in Children)
                b.Delete();
        }

        private void AddChildren(BoatSelectRule parent)
        {
            Db c = new Db(@"SELECT [id], [application], [field], [condition],
                    [string_value], [number_bound1], [number_bound2]
                    FROM [select_rules]
                    WHERE [parent] = @parent");
            Hashtable p = new Hashtable();
            p["parent"] = parent.Id;
            DataTable d = c.GetData(p);
            foreach (DataRow r in d.Rows)
            {
                BoatSelectRule child = new BoatSelectRule();
                child.Id = r["id"] as Guid?;
                if (r["field"] != DBNull.Value)
                {
                    child.Field = (from n in BoatSelectRule.Fields 
                                   where n.Name.Equals(r["field"] as string) 
                                   select n).First<Field>();
                    child.Condition = (ConditionType)System.Enum.GetValues(typeof(ConditionType)).GetValue((int)r["condition"]);
                    child.Rule = RuleType.Simple;
                    child.StringValue = r["string_value"] as string;
                    child.Bound1 = r["number_bound1"] as decimal?;
                    child.Bound2 = r["number_bound2"] as decimal?;
                }
                else
                {
                    child.Application = (Apply)System.Enum.GetValues(typeof(Apply)).GetValue((int)r["application"]);
                    AddChildren(child);
                }
                parent.Add(child);
            }
        }

        public static List<Field> Fields = new List<Field>() { 
            new Rules.Field("Boat class", typeof(string)),
            new Rules.Field("Dinghy", typeof(bool)),
            new Rules.Field("Open handicap", typeof(int))
        };

        public BoatSelectRule Parent { get; set; }

        public void Add(BoatSelectRule bsr)
        {
            bsr.Parent = this;
            _children.Add(bsr);
        }

        public void RemoveFromParent()
        {
            Parent._children.Remove(this);
        }

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
        public Apply Application { get; set; }
        private List<BoatSelectRule> _children = new List<BoatSelectRule>();
        public ReadOnlyCollection<BoatSelectRule> Children
        {
            get { return new ReadOnlyCollection<BoatSelectRule>(_children); }
        }

        //
        // Add rule to database.
        //
        public void Save()
        {
            Db c;
            Hashtable p = new Hashtable();
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
        }
    }
}
