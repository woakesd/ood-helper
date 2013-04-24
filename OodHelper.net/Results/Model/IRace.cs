using System;
namespace OodHelper.Results.Model
{
    interface IRace
    {
        ICalendarEvent Event { get; set; }
        System.Collections.Generic.IList<IEntry> EventEntries { get; set; }
    }
}
