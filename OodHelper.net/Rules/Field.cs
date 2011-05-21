using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.Rules
{
    public class Field
    {
        public string Name { get; set; }
        public string Column { get; set; }
        public IEnumerable<ConditionType> conditions { get; set; }
        public Type FieldType { get; private set; }

        public Field(string name, string column, Type entityType)
        {
            Name = name;
            FieldType = entityType;
            Column = column;

            if (entityType == typeof(bool))
            {
                conditions = new ConditionType[] { ConditionType.True, ConditionType.False };
            }
            else if (entityType == typeof(string))
            {
                conditions = new ConditionType[] { ConditionType.Not_Equal, ConditionType.Equals, 
                    ConditionType.Start_With, ConditionType.Contains, ConditionType.Ends_With };
            }
            else if (entityType == typeof(int))
            {
                conditions = new ConditionType[] { ConditionType.Equals, ConditionType.Not_Equal, 
                    ConditionType.Between, ConditionType.Greater_Than, ConditionType.Greater_Than_Or_Equal_To, 
                    ConditionType.Less_Than, ConditionType.Less_Than_Or_Equal_To };
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
