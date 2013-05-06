using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OodHelper.Results.Model
{
    public class Race : IRace
    {
        public IList<IEntry> EventEntries { get; set; }
        public ICalendarEvent Event { get; set; }

        public Race(ICalendarEvent Calendar, IList<IEntry> Entries)
        {
            Event = Calendar;
            EventEntries = Entries;
        }

        public void Save()
        {
            System.Windows.MessageBox.Show("Save clicked!");
        }

        public void Publish()
        {
            System.Windows.MessageBox.Show("Publish clicked!");
        }
    }
}
