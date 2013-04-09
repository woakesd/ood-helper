using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OodHelper.Results.Model;

namespace NunitTests
{
    public class Event: ICalendarEvent
    {
        string ICalendarEvent.course
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        string ICalendarEvent.eventClass
        {
            get { throw new NotImplementedException(); }
        }

        string ICalendarEvent.eventName
        {
            get { throw new NotImplementedException(); }
        }

        int? ICalendarEvent.extension
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        CalendarEvent.Handicappings ICalendarEvent.handicapping
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        int? ICalendarEvent.laps
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        string ICalendarEvent.ood
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        CalendarEvent.RaceTypes ICalendarEvent.racetype
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        int ICalendarEvent.rid
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        DateTime? ICalendarEvent.start_date
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        int? ICalendarEvent.time_limit_delta
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        DateTime? ICalendarEvent.time_limit_fixed
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        CalendarEvent.TimeLimitTypes ICalendarEvent.time_limit_type
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        void ICalendarEvent.Update()
        {
            throw new NotImplementedException();
        }

        string ICalendarEvent.wind_direction
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        string ICalendarEvent.wind_speed
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
