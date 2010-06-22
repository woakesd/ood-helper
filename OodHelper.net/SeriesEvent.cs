using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class SeriesEvent
    {
        public Dictionary<int, SeriesEntry> Entrants;
        private int mNumberOfRacingEntries;

        public int NumberOfRacingEntries
        {
            get { return mNumberOfRacingEntries; }
        }

        private int mRid;

        public int Rid
        {
            get { return mRid; }
        }

        public SeriesEvent(int rid)
        {
            Entrants = new Dictionary<int, SeriesEntry>();
            mNumberOfRacingEntries = 0;
            mRid = rid;
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
        }
    }
}
