using System;
using System.Collections.Generic;

namespace OodHelper.Results.Model
{
    public interface IRace
    {
        ICalendarEvent Event { get; set; }
        IList<IEntry> EventEntries { get; set; }
        void AddNotes();
        void Save();
        void Publish();
        void RefreshHandicaps();
    }
}
