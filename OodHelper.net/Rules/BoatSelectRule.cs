using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net.Rules
{
    class BoatSelectRule
    {
        public enum RuleType { Simple, Compound }
        public enum Apply { Any, All }
        public enum ConditionType { IsEqual, IsLessThan, IsLessThanOrEqual, IsGreaterThan, IsGreaterThanOrEqual, 
            Contains, StartWith, EndsWith, IsBetween }

        public RuleType Rule { get; set; }

        //
        // For simple rules the following will be used.
        //
        public string Entity { get; set; }
        public string Field { get; set; }
        public ConditionType Condition { get; set; }
        public object Bound1 { get; set; }
        public object Bound2 { get; set; }

        //
        // For a compound rule only these will be used.
        //
        public Apply Application { get; set; }
        List<BoatSelectRule> Children;
    }
}
