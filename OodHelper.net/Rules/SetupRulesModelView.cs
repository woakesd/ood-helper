using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net.Rules
{
    class SelectRuleModelView
    {
        public SelectRuleModelView(BoatSelectRule rule)
            : this(rule, null)
        {
        }

        public SelectRuleModelView(BoatSelectRule rule, SelectRuleModelView parent)
        {
            _rule = rule;
            _parent = parent;
            _children = new List<SelectRuleModelView>(
                (from child in rule.Children
                 select new SelectRuleModelView(child, this)).ToList<SelectRuleModelView>());

            _entityFields.Add("Boat class", "boat.boatclass");
            _entityFields.Add("Is dinghy", "boat.dinghy");
            _entityFields.Add("Local Boat", "boat.distance");
        }

        public bool HasChildren { get { return (_children.Count > 0); } }
        public bool HasNoChildren { get { return (_children.Count == 0); } }

        BoatSelectRule _rule;
        SelectRuleModelView _parent;
        List<SelectRuleModelView> _children;

        public List<SelectRuleModelView> Children { get { return _children; } }

        Dictionary<string, string> _entityFields = new Dictionary<string,string>();

        public List<string> EntityFields { get { return _entityFields.Keys.ToList<string>(); } }

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
