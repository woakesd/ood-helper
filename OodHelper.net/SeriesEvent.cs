using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper
{
    [Svn("$Id$")]
    public class SeriesEvent
    {
        public Dictionary<int, SeriesEntry> Entrants;
        private int mNumberOfRacingEntries;
        private int mNumberFinishers;

        public int NumberOfRacingEntries
        {
            get { return mNumberOfRacingEntries; }
        }

        public int NumberOfFinishers
        {
            get { return mNumberFinishers; }
        }

        private int mRid;
        private DateTime mDate;

        public int Rid
        {
            get { return mRid; }
        }

        public DateTime Date
        {
            get { return mDate; }
        }

        public SeriesEvent(int rid, DateTime date)
        {
            Entrants = new Dictionary<int, SeriesEntry>();
            mNumberOfRacingEntries = 0;
            mNumberFinishers = 0;
            mRid = rid;
            mDate = date;
        }

        public void AddEntry(SeriesEntry entrant)
        {
            Entrants.Add(entrant.bid, entrant);
            switch (entrant.code)
            {
                case "DNC":
                case "OOD":
                case "RSC":
                    break;
                default:
                    mNumberOfRacingEntries++;
                    break;
            }
            if (entrant.code == null || entrant.code == string.Empty)
                mNumberFinishers++;
        }
    }
}
