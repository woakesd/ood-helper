using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OodHelper.Results.Model
{
    public class Race
    {
        readonly int _rid;

        public IList<Entry> Entries { get; set; }
        public Calendar Calendar { get; set; }

        public Race(int Rid)
        {
            _rid = Rid;
            Calendar = new Calendar(Rid);
        }
    }
}
