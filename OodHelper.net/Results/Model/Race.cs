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

        public IList<IEntry> Entries { get; set; }
        public ICalendarEvent Event { get; set; }

        public Race()
        {
        }

        public Race(int Rid)
        {
            _rid = Rid;
            Event = new CalendarEvent(Rid);
        }
    }
}
