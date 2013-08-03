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

        public void AddNotes()
        {
            System.Windows.MessageBox.Show("Add notes clicked!");
        }

        /// <summary>
        /// Save pushes the results to the database and calls calculate.
        /// </summary>
        public void Save()
        {
            Event.Update();
            foreach (IEntry _entry in EventEntries)
            {
                _entry.SaveChanges();
            }
        }

        public void Publish()
        {
            System.Windows.MessageBox.Show("Publish clicked!");
        }

        public void RefreshHandicaps()
        {
            System.Windows.MessageBox.Show("Refresh Handicaps clicked!");
        }
    }
}
