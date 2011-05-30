using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace OodHelper.Maintain
{
    public class NamedValuePair
    {
        private string _txt;
        public string Txt { get; set; }
        private string _val;
        public string Val { get { return _val; } set { _val = value; } }

        public NamedValuePair(string txt, string val)
        {
            Txt = txt;
            Val = val;
        }
    }
}
