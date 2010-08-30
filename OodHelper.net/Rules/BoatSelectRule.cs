using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net.Rules
{
    public enum RuleType { Simple, Compound }
    public enum Apply { Any, All }
    public enum ConditionType
    {
        Is_Equal_To, Is_Less_Than, Is_Less_Than_Or_Equal_To, Is_Greater_Than, Is_Greater_Than_Or_Equal_To, 
        Contains, Start_With, Ends_With, Is_Between
    }
    public enum EventType { MembersOnly, Open }

    class BoatSelectRule
    {
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
        private List<BoatSelectRule> _children = new List<BoatSelectRule>();
        public List<BoatSelectRule> Children
        {
            get { return _children; }
        }
    }
}
