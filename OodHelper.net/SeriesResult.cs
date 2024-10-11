using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OodHelper
{
    public class SeriesResult
    {
        private readonly int[] _discardProfile;
        private readonly Dictionary<int, BoatSeriesResult> _resultsLookUp = new Dictionary<int, BoatSeriesResult>();
        private IList<int> _bids;
        private IList<int> _rids;
        public List<BoatSeriesResult> Results = new List<BoatSeriesResult>();
        public Dictionary<int, SeriesEvent> SeriesData;

        /// <summary>
        ///     Create series result object, pass in rows of data each row contain column bid, with points
        ///     or codes for individual races in cols r1, r2 etc
        /// </summary>
        public SeriesResult(int sid, string division, Dictionary<int, SeriesEvent> seriesData, int[] discardProfile)
        {
            SeriesData = seriesData;
            _discardProfile = discardProfile;
            Sid = sid;
            Division = division;
        }

        public SeriesResult(int sid, string division, Dictionary<int, SeriesEvent> seriesData)
            : this(sid, division, seriesData, new[] {0, 1})
        {
        }

        public string SeriesName { get; set; }
        public int Sid { get; set; }
        public string Division { get; set; }

        public void Score()
        {
            _rids = new List<int>(SeriesData.Keys);

            var discardCount = 0;
            if (_rids != null)
                discardCount = _discardProfile.Length < _rids.Count
                    ? _discardProfile[_discardProfile.Length - 1]
                    : _discardProfile[_rids.Count - 1];

            _bids = new List<int>();
            foreach (
                var bid in
                    from seriesEvent in SeriesData.Values
                    from bid in seriesEvent.Entrants.Keys
                    where !_bids.Contains(bid)
                    select bid)
            {
                _bids.Add(bid);
                var bsr = new BoatSeriesResult(bid);
                Results.Add(bsr);
                _resultsLookUp.Add(bid, bsr);
            }

            //
            // For each boat that has entered an event in the series check for any
            // they missed and add a DNC.
            //
            foreach (var bid in _bids)
            {
                foreach (var e in SeriesData.Values)
                {
                    if (!e.Entrants.ContainsKey(bid))
                    {
                        var se = new SeriesEntry
                        {
                            date = e.Date,
                            code = "DNC",
                            rid = e.Rid,
                            bid = bid,
                            points = _bids.Count + 1
                        };
                        e.AddEntry(se);
                    }
                }
            }

            foreach (var bid in _bids)
            {
                //
                // For each boat loop through the events looking for codes that can be assigned
                // points based on number of entries.
                //
                foreach (var seriesEvent in SeriesData.Values)
                {
                    if (!seriesEvent.Entrants.ContainsKey(bid)) continue;

                    var seriesEntry = seriesEvent.Entrants[bid];
                    switch (seriesEntry.code)
                    {
                        case "DNF":
                        case "DNS":
                        case "RET":
                        case "RTD":
                        case "DSQ":
                        case "DNE":
                        case "OCS":
                        case "RAF":
                            seriesEntry.points = seriesEvent.NumberOfRacingEntries + 1;
                            break;
                        case "DNC":
                            seriesEntry.points = _bids.Count + 1;
                            break;
                    }
                }

                var events = (from seriesEvent in SeriesData.Values
                    where seriesEvent.Entrants.ContainsKey(bid)
                    select seriesEvent.Entrants[bid]).ToList();
                events.Sort(new PreAverageComparer());

                double averagePoints;
                var total = 0.0;

                var cnt = 0;
                var countable = events.Count - discardCount;
                for (var i = 0; i < events.Count; i++)
                {
                    if (!events[i].IsAverageScore && i < countable)
                    {
                        total += events[i].Points;
                        cnt++;
                    }
                    if (i < countable) continue;

                    events[i].discard = true;
                    if (cnt != 0) continue;

                    total = events[i].Points;
                    cnt++;
                }
                if (cnt == 0)
                {
                    total = _bids.Count + 1;
                    cnt = 1;
                }
                averagePoints = total / cnt;

                //
                // For each boat loop through the events and add up all non DNC and average score codes.
                //
                foreach (
                    var seriesEntry in
                        from seriesEvent in SeriesData.Values
                        where seriesEvent.Entrants.ContainsKey(bid)
                        select seriesEvent.Entrants[bid])
                {
                    switch (seriesEntry.code)
                    {
                        case "DNC":
                        case "OOD":
                        case "RSC":
                        case "AVG":
                            break;
                        default:
                            var bsr = _resultsLookUp[bid];
                            bsr.Total += seriesEntry.Points;
                            bsr.Count++;
                            break;
                    }
                }

                //
                // Loop through assigning averages.
                //
                foreach (var entry in SeriesData.Values.Select(seriesEvent => seriesEvent.Entrants[bid]).Where(seriesEntry => seriesEntry.IsAverageScore))
                {
                    entry.points = averagePoints is double.NaN ? _bids.Count + 1 : averagePoints;
                }
            }

            var boatResults = new Dictionary<int, Dictionary<int, SeriesEntry>>();

            //
            // Next get event entries per boat.
            //
            foreach (var bid in _bids)
            {
                foreach (var e in SeriesData.Values)
                {
                    if (!boatResults.ContainsKey(bid))
                        boatResults.Add(bid, new Dictionary<int, SeriesEntry>());
                    var boatEntries = boatResults[bid];
                    if (e.Entrants.ContainsKey(bid))
                        boatEntries.Add(e.Rid, e.Entrants[bid]);
                }
            }

            foreach (var bid in _bids)
            {
                var boatEntries = boatResults[bid];
                var bsr = _resultsLookUp[bid];
                bsr.PerformanceSortedPoints = new List<SeriesEntry>(boatEntries.Values.Where(ent => !ent.Discard));
                bsr.PerformanceSortedPoints.Sort(new PerformanceComparer());
                bsr.DateSortedPoints = new List<SeriesEntry>(boatEntries.Values);
                bsr.DateSortedPoints.Sort(new DateComparer());
                foreach (var entry in bsr.PerformanceSortedPoints)
                    bsr.Net += entry.Points;
            }

            Results.Sort(new NettComparer());

            var place = 1;
            foreach (var boat in Results)
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
            try
            {
                var p = new Hashtable
                {
                    ["sid"] = Sid,
                    ["division"] = Division
                };
                using (var c = new Db(@"DELETE FROM series_results
                WHERE [sid] = @sid
                AND [division] = @division"))
                {
                    c.ExecuteNonQuery(p);
                }
                using (var c = new Db(@"INSERT INTO series_results
                ([sid], [bid], [division], [entered], [gross], [nett], [place])
                VALUES (@sid, @bid, @division, @entered, @gross, @nett, @place)"))
                {
                    foreach (var bsr in Results)
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
            catch (Exception e)
            {
                ErrorLogger.LogException(e);
            }
        }

        private class PreAverageComparer : IComparer<SeriesEntry>
        {
            public int Compare(SeriesEntry? x, SeriesEntry? y)
            {
                if (x!.code != y!.code)
                {
                    if (x.code == "DNE")
                        return -1;
                    if (y.code == "DNE")
                        return 1;
                }
                if (x.IsAverageScore ^ y.IsAverageScore)
                {
                    if (x.IsAverageScore)
                        return -1;
                    if (y.IsAverageScore)
                        return 1;
                }
                return x.Points.CompareTo(y.Points);
            }
        }
    }

    public class BoatSeriesResult
    {
        public int Bid;
        public int Count;
        public List<SeriesEntry> DateSortedPoints;
        public double Net;
        public List<SeriesEntry> PerformanceSortedPoints;
        public int Place;
        public double Total;

        public BoatSeriesResult(int b)
        {
            Bid = b;
            Total = 0.0;
            Net = 0.0;
            Count = 0;
        }
    }

    internal class NettComparer : IComparer<BoatSeriesResult>
    {
        public int Compare(BoatSeriesResult x, BoatSeriesResult y)
        {
            if (x.Bid == y.Bid) return 0;

            if (x.Net > y.Net)
                return 1;
            if (x.Net < y.Net)
                return -1;
            if (!double.IsNaN(x.Net) && double.IsNaN(y.Net))
                return 1;
            if (double.IsNaN(x.Net) && !double.IsNaN(y.Net))
                return -1;

            for (var i = 0; i < x.PerformanceSortedPoints.Count && i < y.PerformanceSortedPoints.Count; i++)
            {
                if (!x.PerformanceSortedPoints[i].Discard && !y.PerformanceSortedPoints[i].Discard
                    && x.PerformanceSortedPoints[i].Points > y.PerformanceSortedPoints[i].Points)
                    return 1;
                if (!x.PerformanceSortedPoints[i].Discard && !y.PerformanceSortedPoints[i].Discard
                    && x.PerformanceSortedPoints[i].Points < y.PerformanceSortedPoints[i].Points)
                    return -1;
            }

            for (var i = x.DateSortedPoints.Count - 1; i >= 0; i--)
            {
                if (x.DateSortedPoints[i].Points > y.DateSortedPoints[i].Points)
                    return 1;
                if (x.DateSortedPoints[i].Points < y.DateSortedPoints[i].Points)
                    return -1;
            }

            return 0;
        }
    }

    internal class PerformanceComparer : IComparer<SeriesEntry>
    {
        public int Compare(SeriesEntry x, SeriesEntry y)
        {
            if (x.Points > y.Points)
                return 1;
            if (x.Points < y.Points)
                return -1;
            if (x.date < y.date)
                return 1;
            if (x.date > y.date)
                return -1;
            return 0;
        }
    }

    internal class DateComparer : IComparer<SeriesEntry>
    {
        public int Compare(SeriesEntry x, SeriesEntry y)
        {
            if (x.date > y.date)
                return 1;
            if (x.date < y.date)
                return -1;
            return 0;
        }
    }
}