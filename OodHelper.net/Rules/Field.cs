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
                conditions = new ConditionType[] { ConditionType.NotEqual, ConditionType.Equals, 
                    ConditionType.StartWith, ConditionType.Contains, ConditionType.EndsWith };
            }
            else if (entityType == typeof(int))
            {
                conditions = new ConditionType[] { ConditionType.Equals, ConditionType.NotEqual, 
                    ConditionType.Between, ConditionType.GreaterThan, ConditionType.GreaterThanOrEqualTo, 
                    ConditionType.LessThan, ConditionType.LessThanOrEqualTo };
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
