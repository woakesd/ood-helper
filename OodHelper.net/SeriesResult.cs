using System;
using System.Collections;
using System.Collections.Generic;

namespace OodHelper
{
    public class SeriesResult
    {
        private readonly int[] _discardProfile;
        public Dictionary<int, SeriesEvent> SeriesData;
        private int[] _bids;
        private int[] _rids;
        public List<BoatSeriesResult> Results = new List<BoatSeriesResult>();
        private readonly Dictionary<int, BoatSeriesResult> _resultsLookUp = new Dictionary<int, BoatSeriesResult>();
        public string SeriesName { get; set; }
        public int Sid { get; set; }
        public string Division { get; set; }

        /// <summary>
        /// Create series result object, pass in rows of data each row contain column bid, with points
        /// or codes for individual races in cols r1, r2 etc
        /// </summary>
        public SeriesResult(int sid, string division, Dictionary<int, SeriesEvent> seriesData, int[] discardProfile)
        {
            SeriesData = seriesData;
            _discardProfile = discardProfile;
            Sid = sid;
            Division = division;
        }

        public SeriesResult(int sid, string division, Dictionary<int, SeriesEvent> seriesData)
            : this(sid, division, seriesData, new[] { 0, 1 })
        {
        }

        public void Score()
        {
            ArrayList b = new ArrayList();
            //
            // assign DNC scores
            //
            ArrayList rids = new ArrayList(SeriesData.Keys);
            _rids = rids.ToArray(typeof(int)) as int[];

            foreach (SeriesEvent e in SeriesData.Values)
            {
                foreach (int bid in e.Entrants.Keys)
                    if (!b.Contains(bid))
                    {
                        b.Add(bid);
                        BoatSeriesResult bsr = new BoatSeriesResult(bid);
                        Results.Add(bsr);
                        _resultsLookUp.Add(bid, bsr);
                    }
            }
            _bids = b.ToArray(typeof(int)) as int[];

            if (_bids != null)
            {
                foreach (int bid in _bids)
                {
                    //
                    // For each boat loop through the events looking for codes that can be assigned
                    // points based on number of entries.
                    //
                    foreach (SeriesEvent e in SeriesData.Values)
                    {
                        if (e.Entrants.ContainsKey(bid))
                        {
                            SeriesEntry se = e.Entrants[bid];
                            switch (se.code)
                            {
                                case "DNF":
                                case "DNS":
                                case "RET":
                                case "RTD":
                                case "DSQ":
                                case "OCS":
                                case "RAF":
                                    se.points = e.NumberOfRacingEntries + 1;
                                    break;
                                case "DNC":
                                    se.points = _bids.Length + 1;
                                    break;
                            }
                        }
                    }

                    //
                    // For each boat loop through the events and add up all non DNC and average score codes.
                    //
                    foreach (SeriesEvent e in SeriesData.Values)
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
                                    BoatSeriesResult bsr = _resultsLookUp[bid];
                                    bsr.Total += se.Points;
                                    bsr.Count++;
                                    break;
                            }
                        }
                    }

                    //
                    // Loop through assigning averages.
                    //
                    foreach (SeriesEvent e in SeriesData.Values)
                    {
                        if (e.Entrants.ContainsKey(bid))
                        {
                            SeriesEntry se = e.Entrants[bid];
                            switch (se.code.ToUpper())
                            {
                                case "OOD":
                                case "AVG":
                                case "RSC":
                                    if (_resultsLookUp[bid].Count > 0)
                                    {
                                        BoatSeriesResult bsr = _resultsLookUp[bid];
                                        se.points = bsr.Total / bsr.Count;
                                    }
                                    else
                                    //
                                    // if no races have been entered then DNC is average!
                                    //
                                        se.points = _bids.Length;
                                    break;
                            }
                        }
                    }
                }

                //
                // For each boat that has entered an event in the series check for any
                // they missed and add a DNC.
                //
                foreach (int bid in _bids)
                {
                    foreach (SeriesEvent e in SeriesData.Values)
                    {
                        if (!e.Entrants.ContainsKey(bid))
                        {
                            var se = new SeriesEntry
                            {
                                date = e.Date,
                                code = "DNC",
                                rid = e.Rid,
                                bid = bid,
                                points = _bids.Length + 1
                            };
                            e.AddEntry(se);
                        }
                    }
                }

                var boatResults = new Dictionary<int, Dictionary<int, SeriesEntry>>();
            
                //
                // Next get event entries per boat.
                //
                foreach (int bid in _bids)
                {
                    foreach (SeriesEvent e in SeriesData.Values)
                    {
                        if (!boatResults.ContainsKey(bid))
                            boatResults.Add(bid, new Dictionary<int, SeriesEntry>());
                        var boatEntries = boatResults[bid];
                        if (e.Entrants.ContainsKey(bid))
                            boatEntries.Add(e.Rid, e.Entrants[bid]);
                    }
                }

                int discardCount = 0;
                if (_rids != null)
                    discardCount = _discardProfile.Length < _rids.Length ? _discardProfile[_discardProfile.Length-1] : _discardProfile[_rids.Length-1];

                foreach (var bid in _bids)
                {
                    var boatEntries = boatResults[bid];
                    var bsr = _resultsLookUp[bid];
                    bsr.PerformanceSortedPoints = new List<SeriesEntry>(boatEntries.Values);
                    bsr.PerformanceSortedPoints.Sort(new PerformanceComparer());
                    bsr.DateSortedPoints = new List<SeriesEntry>(boatEntries.Values);
                    bsr.DateSortedPoints.Sort(new DateComparer());
                    for (int i = 0; i < bsr.PerformanceSortedPoints.Count - discardCount; i++)
                        bsr.Net += bsr.PerformanceSortedPoints[i].Points;
                    for (int i = bsr.PerformanceSortedPoints.Count - discardCount; i < bsr.PerformanceSortedPoints.Count; i++)
                        bsr.PerformanceSortedPoints[i].discard = true;
                }
            }

            Results.Sort(new NettComparer());

            int place = 1;
            foreach (BoatSeriesResult boat in Results)
            {
                boat.Place = place++;
            }

            //
            // Save to database replacing existing values
            //
            SaveResults();
        }

        private void SaveResults()
        {
            Hashtable p = new Hashtable();
            p["sid"] = Sid;
            p["division"] = Division;
            using (Db c = new Db(@"DELETE FROM series_results
                WHERE [sid] = @sid
                AND [division] = @division"))
            {
                c.ExecuteNonQuery(p);
            }
            using (Db c = new Db(@"INSERT INTO series_results
                ([sid], [bid], [division], [entered], [gross], [nett], [place])
                VALUES (@sid, @bid, @division, @entered, @gross, @nett, @place)"))
            {
                foreach (BoatSeriesResult bsr in Results)
                {
                    p["bid"] = bsr.Bid;
                    p["entered"] = bsr.Count;
                    p["gross"] = bsr.Total;
                    p["nett"] = bsr.Net;
                    p["place"] = bsr.Place;
                    c.ExecuteNonQuery(p);
                }
            }
        }
    }

    public class BoatSeriesResult
    {
        public BoatSeriesResult(int b)
        {
            Bid = b;
            Total = 0.0;
            Net = 0.0;
            Count = 0;
        }

        public int Bid;
        public double Total;
        public double Net;
        public int Count;
        public List<SeriesEntry> PerformanceSortedPoints;
        public List<SeriesEntry> DateSortedPoints;
        public int Place;
    }

    class NettComparer : IComparer<BoatSeriesResult>
    {
        public int Compare(BoatSeriesResult x, BoatSeriesResult y)
        {
            if (x.Bid == y.Bid) return 0;

            if (x.Net > y.Net)
                return 1;
            else if (x.Net < y.Net)
                return -1;
            else if (!Double.IsNaN(x.Net) && Double.IsNaN(y.Net))
                return 1;
            else if (Double.IsNaN(x.Net) && !Double.IsNaN(y.Net))
                return -1;

            for (int i = 0; i < x.PerformanceSortedPoints.Count && i < y.PerformanceSortedPoints.Count; i++)
            {
                if (!x.PerformanceSortedPoints[i].Discard && !y.PerformanceSortedPoints[i].Discard 
                    && x.PerformanceSortedPoints[i].Points > y.PerformanceSortedPoints[i].Points)
                    return 1;
                else if (!x.PerformanceSortedPoints[i].Discard && !y.PerformanceSortedPoints[i].Discard 
                    && x.PerformanceSortedPoints[i].Points < y.PerformanceSortedPoints[i].Points)
                    return -1;
            }

            for (int i = x.DateSortedPoints.Count - 1; i >= 0; i--)
            {
                if (x.DateSortedPoints[i].Points > y.DateSortedPoints[i].Points)
                    return 1;
                else if (x.DateSortedPoints[i].Points < y.DateSortedPoints[i].Points)
                    return -1;
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
