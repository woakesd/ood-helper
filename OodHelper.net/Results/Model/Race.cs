﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OodHelper.Results.Model
{
    public class CommandListItem
    {
        public string Text { get; set; }
        public RelayCommand Command { get; set; }
        public object CommandParameter { get; set; }
    }
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

        public void Save()
        {
            System.Windows.MessageBox.Show("Save clicked!");
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
