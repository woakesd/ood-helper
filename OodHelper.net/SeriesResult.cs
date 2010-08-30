using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace OodHelper.net
{
    [Svn("$Id$")]
    public class SeriesResult
    {
        private int[] _DiscardProfile;
        public Dictionary<int, SeriesEvent> _SeriesData;
        private int[] _Bids;
        private int[] _Rids;
        public List<BoatSeriesResult> _Results = new List<BoatSeriesResult>();
        private Dictionary<int, BoatSeriesResult> _ResultsLookUp = new Dictionary<int, BoatSeriesResult>();
        public string SeriesName { get; set; }

        /// <summary>
        /// Create series result object, pass in rows of data each row contain column bid, with points
        /// or codes for individual races in cols r1, r2 etc
        /// </summary>
        /// <param name="SeriesData"></param>
        public SeriesResult(Dictionary<int, SeriesEvent> SeriesData, int[] DiscardProfile)
        {
            _SeriesData = SeriesData;
            _DiscardProfile = DiscardProfile;
        }

        public SeriesResult(Dictionary<int, SeriesEvent> SeriesData)
            : this(SeriesData, new int[] { 0, 1 })
        {
        }

        public void Score()
        {
            ArrayList b = new ArrayList();
            //
            // assign DNC scores
            //
            ArrayList rids = new ArrayList(_SeriesData.Keys);
            _Rids = rids.ToArray(typeof(int)) as int[];

            foreach (SeriesEvent e in _SeriesData.Values)
            {
                foreach (int bid in e.Entrants.Keys)
                    if (!b.Contains(bid))
                    {
                        b.Add(bid);
                        BoatSeriesResult bsr = new BoatSeriesResult(bid);
                        _Results.Add(bsr);
                        _ResultsLookUp.Add(bid, bsr);
                    }
            }
            _Bids = b.ToArray(typeof(int)) as int[];

            foreach (int bid in _Bids)
            {
                //
                // For each boat loop through the events looking for codes that can be assigned
                // points based on number of entries.
                //
                foreach (SeriesEvent e in _SeriesData.Values)
                {
                    if (e.Entrants.ContainsKey(bid))
                    {
                        SeriesEntry se = e.Entrants[bid];
                        switch (se.code)
                        {
                            case "DNF":
                            case "DNS":
                            case "RET":
                            case "DSQ":
                            case "OCS":
                            case "RAF":
                                se.points = e.NumberOfRacingEntries + 1;
                                break;
                            case "DNC":
                                se.points = _Bids.Length + 1;
                                break;
                        }
                    }
                }

                //
                // For each boat loop through the events and add up all non DNC and average score codes.
                //
                foreach (SeriesEvent e in _SeriesData.Values)
                {
                    if (e.Entrants.ContainsKey(bid))
                    {
                        SeriesEntry se = e.Entrants[bid];
                        switch (se.code)
                        {
                            case "DNC":
                            case "OOD":
                            case "RSC":
                            case "AVG":
                                break;
                            default:
                                BoatSeriesResult bsr = _ResultsLookUp[bid];
                                bsr.total += se.Points;
                                bsr.count++;
                                break;
                        }
                    }
                }

                //
                // Loop through assigning averages.
                //
                foreach (SeriesEvent e in _SeriesData.Values)
                {
                    if (e.Entrants.ContainsKey(bid))
                    {
                        SeriesEntry se = e.Entrants[bid];
                        switch (se.code.ToUpper())
                        {
                            case "OOD":
                            case "AVG":
                            case "RSC":
                                if (_ResultsLookUp[bid].count > 0)
                                {
                                    BoatSeriesResult bsr = _ResultsLookUp[bid];
                                    se.points = bsr.total / bsr.count;
                                }
                                else
                                    //
                                    // if no races have been entered then DNC is average!
                                    //
                                    se.points = _Bids.Length;
                                break;
                        }
                    }
                }
            }

            //
            // For each boat that has entered an event in the series check for any
            // they missed and add a DNC.
            //
            foreach (int bid in _Bids)
            {
                foreach (SeriesEvent e in _SeriesData.Values)
                {
                    if (!e.Entrants.ContainsKey(bid))
                    {
                        SeriesEntry se = new SeriesEntry();
                        se.date = e.Date;
                        se.code = "DNC";
                        se.rid = e.Rid;
                        se.bid = bid;
                        se.points = _Bids.Length + 1;
                        e.AddEntry(se);
                    }
                }
            }

            Dictionary<int, Dictionary<int, SeriesEntry>> BoatResults = new Dictionary<int, Dictionary<int, SeriesEntry>>();
            
            //
            // Next get event entries per boat.
            //
            foreach (int bid in _Bids)
            {
                foreach (SeriesEvent e in _SeriesData.Values)
                {
                    if (!BoatResults.ContainsKey(bid))
                        BoatResults.Add(bid, new Dictionary<int, SeriesEntry>());
                    Dictionary<int, SeriesEntry> BoatEntries = BoatResults[bid];
                    if (e.Entrants.ContainsKey(bid))
                        BoatEntries.Add(e.Rid, e.Entrants[bid]);
                }
            }

            int discardCount = 0;
            if (_DiscardProfile.Length < _Rids.Length)
                discardCount = _DiscardProfile[_DiscardProfile.Length-1];
            else
                discardCount = _DiscardProfile[_Rids.Length-1];

            foreach (int bid in _Bids)
            {
                Dictionary<int, SeriesEntry> BoatEntries = BoatResults[bid];
                BoatSeriesResult bsr = _ResultsLookUp[bid];
                bsr.performanceSortedPoints = new List<SeriesEntry>(BoatEntries.Values);
                bsr.performanceSortedPoints.Sort(new PerformanceComparer());
                bsr.dateSortedPoints = new List<SeriesEntry>(BoatEntries.Values);
                bsr.dateSortedPoints.Sort(new DateComparer());
                for (int i = 0; i < bsr.performanceSortedPoints.Count - discardCount; i++)
                    bsr.nett += bsr.performanceSortedPoints[i].Points;
                for (int i = bsr.performanceSortedPoints.Count - discardCount; i < bsr.performanceSortedPoints.Count; i++)
                    bsr.performanceSortedPoints[i].discard = true;
            }

            _Results.Sort(new NettComparer());

            int place = 1;
            foreach (BoatSeriesResult boat in _Results)
            {
                boat.place = place++;
            }
        }
    }

    public class BoatSeriesResult
    {
        public BoatSeriesResult(int b)
        {
            bid = b;
            total = 0.0;
            nett = 0.0;
            count = 0;
        }

        public int bid;
        public double total;
        public double nett;
        public int count;
        public List<SeriesEntry> performanceSortedPoints;
        public List<SeriesEntry> dateSortedPoints;
        public int place;
        public double points;
    }

    class NettComparer : IComparer<BoatSeriesResult>
    {
        public int Compare(BoatSeriesResult x, BoatSeriesResult y)
        {
            if (x.bid == y.bid) return 0;

            if (x.nett > y.nett)
                return 1;
            else if (x.nett < y.nett)
                return -1;

            for (int i = 0; i < x.performanceSortedPoints.Count && i < y.performanceSortedPoints.Count; i++)
            {
                if (x.performanceSortedPoints[i].Points > y.performanceSortedPoints[i].Points)
                    return 1;
                else if (x.performanceSortedPoints[i].Points < y.performanceSortedPoints[i].Points)
                    return -1;
            }

            for (int i = 0; i < x.dateSortedPoints.Count && i < y.dateSortedPoints.Count; i++)
            {
                if (x.dateSortedPoints[i].Points > y.dateSortedPoints[i].Points)
                    return -1;
                else if (x.dateSortedPoints[i].Points < y.dateSortedPoints[i].Points)
                    return 1;
            }
            
            return 0;
        }
    }

    class PerformanceComparer : IComparer<SeriesEntry>
    {
        public int Compare(SeriesEntry x, SeriesEntry y)
        {
            if (x.Points > y.Points)
                return 1;
            else if (x.Points < y.Points)
                return -1;
            if (x.date < y.date)
                return 1;
            else if (x.date > y.date)
                return -1;
            return 0;
        }
    }

    class DateComparer : IComparer<SeriesEntry>
    {
        public int Compare(SeriesEntry x, SeriesEntry y)
        {
            if (x.date > y.date)
                return 1;
            else if (x.date < y.date)
                return -1;
            return 0;
        }
    }

}
