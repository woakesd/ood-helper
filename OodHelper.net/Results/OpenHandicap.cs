﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace OodHelper.Results
{
    internal class OpenHandicap : IRaceScore
    {
        private BackgroundWorker _back;
        protected DataTable Racedata;
        protected Db Racedb;
        public CalendarModel.RaceTypes RaceType { get; private set; }
        public int Rid { get; private set; }

        public double SlowLimit { get; private set; }

        public double FastLimit { get; private set; }

        private DateTime? TimeLimit { get; set; }
        private int? Extension { get; set; }
        public int Finishers { get; private set; }

        public bool Calculated { get; private set; }

        public double StandardCorrectedTime { get; private set; }

        public void Calculate(object sender, DoWorkEventArgs e)
        {
            _back = sender as BackgroundWorker;
            Calculate((int) e.Argument);
        }

        public void Calculate(int r)
        {
            try
            {
                Rid = r;
                var p = new Hashtable {["rid"] = Rid};

                var c =
                    new Db(
                        @"SELECT c.racetype, c.start_date, result_calculated, MAX(last_edit) last_edit, standard_corrected_time,
                CASE time_limit_type
                WHEN 'F' THEN time_limit_fixed
                WHEN 'D' THEN DATEADD(SECOND, time_limit_delta, c.start_date)
                END time_limit, extension
                FROM calendar c LEFT JOIN races r ON c.rid = r.rid
                WHERE c.rid = @rid
                GROUP BY c.racetype, c.start_date, result_calculated, standard_corrected_time, CASE time_limit_type
                WHEN 'F' THEN time_limit_fixed
                WHEN 'D' THEN DATEADD(SECOND, time_limit_delta, c.start_date)
                END, extension");
                var res = c.GetHashtable(p);

                CalendarModel.RaceTypes racetype;
                RaceType = Enum.TryParse(res["racetype"].ToString(), out racetype) ? racetype : CalendarModel.RaceTypes.Undefined;

                var dsct = res["standard_corrected_time"] as double?;
                StandardCorrectedTime = 0;
                if (dsct.HasValue)
                    StandardCorrectedTime = dsct.Value;

                TimeLimit = res["time_limit"] as DateTime?;
                Extension = res["extension"] as int?;

                if (res["result_calculated"] == DBNull.Value || res["last_edit"] != DBNull.Value &&
                    (DateTime) res["result_calculated"] <= (DateTime) res["last_edit"])
                {
                    Racedb = new Db(@"SELECT * FROM races WHERE rid = @rid");
                    Racedata = Racedb.GetData(p);

                    _back?.ReportProgress(0, "Deleting DNC");
                    DeleteDidNotCompete();

                    _back?.ReportProgress(10, "Checking for finishers");
                    if (HasFinishers())
                    {
                        _back?.ReportProgress(20, "Flagging up DNFs");
                        FlagDidNotFinish();
                        _back?.ReportProgress(30, "Initialising");
                        InitialiseFields();
                        _back?.ReportProgress(40, "Do corrected time");
                        CorrectedTime();
                        _back?.ReportProgress(50, "Calculating SCT");
                        CalculateSct();
                        _back?.ReportProgress(60, "Setting places and points");
                        Score();
                        _back?.ReportProgress(70, "Updating rolling handicaps");
                        UpdateHandicaps();
                        _back?.ReportProgress(80, "Commit changes");
                        CommitChanges();
                        _back?.ReportProgress(90, "Updating calendar");

                        c = new Db(@"UPDATE calendar
                        SET result_calculated = GETDATE(),
                        raced = 1
                        WHERE rid = @rid");
                        c.ExecuteNonQuery(p);
                        _back?.ReportProgress(100, "Completed");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex);
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CommitChanges()
        {
            var first = true;
            var sql = new StringBuilder("UPDATE races SET ");
            foreach (DataColumn c in Racedata.Columns)
            {
                if (c.ColumnName != "bid" && c.ColumnName != "rid")
                {
                    if (!first)
                        sql.Append(",");
                    first = false;
                    sql.AppendFormat("{0} = @{0}", c.ColumnName);
                }
            }
            sql.Append(" WHERE rid = @rid AND bid = @bid");
            var d = new Db(sql.ToString());
            var p = new Hashtable();
            foreach (DataRow r in Racedata.Rows)
            {
                foreach (DataColumn c in Racedata.Columns)
                {
                    p[c.ColumnName] = r[c];
                }
                d.ExecuteNonQuery(p);
            }
        }

        private void FlagDidNotFinish()
        {
            var nonFinishers = false;
            //
            // Mark non finishers
            //
            if (TimeLimit.HasValue)
            {
                if (Extension.HasValue)
                    TimeLimit = TimeLimit.Value.AddSeconds(Extension.Value);

                foreach (
                    var r in
                        (from r in Racedata.AsEnumerable() where r.Field<DateTime?>("finish_date") > TimeLimit select r)
                    )
                {
                    r["finish_code"] = "DNF";
                    nonFinishers = true;
                }
            }

            if (nonFinishers)
                MessageBox.Show(
                    "Some boats have finished outside the timelimit\n(plus extension if applicable) and have been marked DNF",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void InitialiseFields()
        {
            //
            // update all boats setting elapsed, corrected, stdcorr, place and pts.
            //
            foreach (var r in (from r in Racedata.AsEnumerable() select r))
            {
                r["elapsed"] = DBNull.Value;
                r["corrected"] = DBNull.Value;
                r["standard_corrected"] = DBNull.Value;
                r["place"] = 999;
                r["points"] = DBNull.Value;
                if (r["rolling_handicap"] == DBNull.Value)
                {
                    var p = new Hashtable
                    {
                        ["bid"] = r["bid"],
                        ["rid"] = Rid
                    };
                    var hc = new Db(@"SELECT r3.new_rolling_handicap
                        FROM races r1
                        INNER JOIN races r2 ON r2.bid = r1.bid AND r2.start_date < r1.start_date
                        INNER JOIN races r3 ON r3.bid = r1.bid
                        WHERE r1.rid = @rid AND r1.bid = @bid
                        GROUP BY r1.bid, r3.start_date, r3.new_rolling_handicap
                        HAVING r3.start_date = MAX(r2.start_date)");
                    var nrh = hc.GetScalar(p) as int?;
                    if (!nrh.HasValue)
                        nrh = (int) r["open_handicap"];
                    r["rolling_handicap"] = nrh.Value;
                }
                var newhc = (int) r["rolling_handicap"];
                if (r["restricted_sail"] != DBNull.Value && (bool) r["restricted_sail"])
                    newhc = (int) Math.Round(newhc/1.04);

                r["new_rolling_handicap"] = newhc;
                r["achieved_handicap"] = r["rolling_handicap"];
                r["performance_index"] = DBNull.Value;
                r["c"] = DBNull.Value;
                r["a"] = "N";

                //
                // if average lap and user enters a finish time and no laps then default to 1.
                //
                if ((RaceType == CalendarModel.RaceTypes.AverageLap
                     || RaceType == CalendarModel.RaceTypes.Hybrid
                     || RaceType == CalendarModel.RaceTypes.HybridOld)
                    && r["laps"] == DBNull.Value && r["finish_date"] != DBNull.Value)
                    r["laps"] = 1;
            }
        }

        private void DeleteDidNotCompete()
        {
            var c = new Db("DELETE FROM races WHERE rid = @rid AND finish_code IN ('DNC', 'BAD')");
            var p = new Hashtable {["rid"] = Rid};
            c.ExecuteNonQuery(p);
        }

        private bool HasFinishers()
        {
            int? count = Racedata.AsEnumerable().Count(r => r.Field<DateTime?>("finish_date") <= TimeLimit);

            Finishers = count.Value;
            if (count.Value > 0)
                return true;

            MessageBox.Show("All boats have finished outside the timelimit\nNo calculation can be performed.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        protected virtual void CorrectedTime()
        {
            var query = (from r in Racedata.AsEnumerable()
                where r.Field<string>("finish_code") != "DNF"
                      && r.Field<string>("finish_code") != "DSQ"
                      && r.Field<string>("finish_code") != "DNE"
                      && r.Field<DateTime?>("start_date") != null
                      && r.Field<DateTime?>("finish_date") != null
                      &&
                      (RaceType != CalendarModel.RaceTypes.HybridOld ||
                       r.Field<DateTime?>("interim_date") != null && r.Field<int?>("laps") != null)
                      &&
                      (RaceType != CalendarModel.RaceTypes.Hybrid ||
                       r.Field<DateTime?>("interim_date") != null && r.Field<int?>("laps") != null)
                      && (RaceType != CalendarModel.RaceTypes.AverageLap || r.Field<int?>("laps") != null)
                select r);

            //
            // Average laps
            //
            var avgLaps = Math.Round(query.Average(r => (r.Field<int?>("laps")) ?? 0), 1);

            //
            // Select all boats and work out elapsed, corrected and stdcorr
            //
            foreach (var dr in query)
            {
                var start = dr["start_date"] as DateTime?;
                var finish = dr["finish_date"] as DateTime?;
                var interim = dr["interim_date"] as DateTime?;
                TimeSpan? fixedPart;
                TimeSpan? averageLapPart;

                if (RaceType == CalendarModel.RaceTypes.HybridOld)
                {
                }

                var elapsed = finish - start;
                dr["elapsed"] = elapsed.Value.TotalSeconds;

                var laps = dr["laps"] as int?;
                if (laps.HasValue && laps.Value == 0) laps = 1;

                var hcap = (int) dr["open_handicap"];

                //
                // if spec is 'a' then this is average lap so corrected times are per lap,
                // otherwise assume everyone did same number of laps.
                //
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.AverageLap:
                        dr["corrected"] = Math.Round(elapsed.Value.TotalSeconds*1000/hcap)/laps.Value;
                        break;
                    case CalendarModel.RaceTypes.HybridOld:
                        fixedPart = interim - start;
                        averageLapPart = finish - interim;
                        dr["corrected"] = Math.Round(fixedPart.Value.TotalSeconds*1000/hcap) +
                                          Math.Round(averageLapPart.Value.TotalSeconds*1000/hcap)/laps.Value;
                        break;
                    case CalendarModel.RaceTypes.Hybrid:
                        fixedPart = interim - start;
                        averageLapPart = finish - interim;
                        dr["corrected"] = Math.Round(fixedPart.Value.TotalSeconds*1000/hcap) +
                                          Math.Round(averageLapPart.Value.TotalSeconds*1000/hcap*avgLaps)/laps.Value;
                        break;
                    case CalendarModel.RaceTypes.FixedLength:
                    case CalendarModel.RaceTypes.TimeGate:
                        dr["corrected"] = Math.Round(elapsed.Value.TotalSeconds*1000/hcap);
                        break;
                }
                dr["standard_corrected"] = dr["corrected"];
                dr["place"] = 0;
            }
        }

        private void CalculateSct()
        {
            //
            // First select the boats in the race that have Portsmouth, Secondary, Recorded or
            // Club numbers (handicaps).  These will be called good boats in the comments.
            //

            var query = from r in Racedata.AsEnumerable()
                where r.Field<int?>("place") == 0
                      && r.Field<string>("handicap_status") != "TN"
                orderby r.Field<double?>("standard_corrected")
                select r;

            //
            // if we have fewer than 2 good boats there is no
            // corrected time.
            //
            var qual = query.Count();
            if (qual < 2)
            {
                StandardCorrectedTime = 0;
            }
            else
            {
                //
                // Loop through the top 2/3 of the good boats and work out their 
                // average corrected time.
                //
                double total = 0;
                var n = (int) Math.Round(qual*0.67);

                for (var i = 0; i < n; i++)
                {
                    total = total + query.ElementAt(i).Field<double?>("standard_corrected").Value;
                    //total = total + (double) UpdateUIDelegate.Rows[_interim]["standard_corrected"];
                }

                var averageCorrectedTime = total/n;

                //
                // The average slow limit is the average corrected time + 5%.
                //
                var avgSlowLimit = averageCorrectedTime*1.05;

                //
                // Next count the number of good boats that beat the slow time.
                // Also sum their standard corrected times.
                //
                var goodBoats = 0;
                StandardCorrectedTime = 0;
                var rows = query.ToArray();
                for (var i = 0; i < rows.Length; i++)
                {
                    var row = rows.ElementAt(i);
                    if (((double) row["standard_corrected"]) < avgSlowLimit)
                    {
                        row["a"] = DBNull.Value;
                        goodBoats++;
                        StandardCorrectedTime = StandardCorrectedTime + (double) row["standard_corrected"];
                    }
                }

                if (goodBoats > 1)
                {
                    //
                    // More than 1 good boat so we have enough to calculate the SCT
                    // which is the average standard corrected time of the good boats
                    // that beat the 5% slow cutoff.
                    //
                    StandardCorrectedTime = Math.Round(StandardCorrectedTime/goodBoats);

                    //
                    // NB These limits are related to the standard corrected time and
                    // are the ones used to when working out new handicaps.
                    //
                    SlowLimit = Math.Round(StandardCorrectedTime*1.05);
                    FastLimit = Math.Round(StandardCorrectedTime*0.95);
                }
                else
                {
                    //
                    // Too few good boats so we cant work this out.
                    //
                    StandardCorrectedTime = 0;
                    SlowLimit = 0;
                    FastLimit = 0;
                }
            }
            var param = new Hashtable
            {
                ["rid"] = Rid,
                ["sct"] = StandardCorrectedTime
            };

            var up = new Db(@"UPDATE calendar SET standard_corrected_time = @sct, raced = 1 WHERE rid = @rid");
            up.ExecuteNonQuery(param);
        }

        private void Score()
        {
            //
            // This routine sets the places column and the pts column.
            //

            //
            // First give everyone a placing.
            //
            var query = (from r in Racedata.AsEnumerable()
                where r.Field<int?>("place") == 0
                orderby r.Field<double?>("corrected")
                select r).ToArray();

            for (var i = 0; i < query.Length; i++)
            {
                //
                // NB tied boats are given the same place, so if two boats are tied in 3rd
                // they both are assigned 3rd.  The boat that follows them is still in 5th.
                //
                if (i == 0)
                    //
                    // first row, so set place.
                    //
                    query.ElementAt(i)["place"] = 1;
                else
                {
                    //
                    // Subsequent rows with non zero elapsed time.
                    //
                    if (query.ElementAt(i - 1)["corrected"].ToString() == query.ElementAt(i)["corrected"].ToString())
                        //
                        // corrected time is the same as the previous row, so use place
                        // from previous row as we are tied.
                        //
                        query.ElementAt(i)["place"] = query.ElementAt(i - 1)["place"];
                    else
                    //
                    // Not tied so assign place.
                    //
                        query.ElementAt(i)["place"] = i + 1;
                }
            }

            //
            // Next we must assign points which are normally the same as the place unless there is a tie
            // in which case the points are shared.  So two boats tied in 3rd will share the points for
            // 3th and 4th and get 3.5 points each.  THIS IS AS PER RRS.
            //
            // If 3 boats tie in 3rd the points are the average of 3, 4 and 5 which is 4, and so on.
            //
            var j = 0;
            while (j < query.Length)
            {
                var k = j + 1;
                double psum = k;
                while (k < query.Count())
                {
                    if (query.ElementAt(j)["place"].ToString() != query.ElementAt(k)["place"].ToString())
                        break;
                    k++;
                    psum = psum + k;
                }

                var avg = Math.Round(psum/(k - j), 2);
                for (var l = j; l < k; l++)
                    query.ElementAt(l)["points"] = avg;
                j = k;
            }
        }

        private void UpdateHandicaps()
        {
            var p = new Hashtable();
            //
            // This routine sets the new rolling handicap column.
            //
            var query = from r in Racedata.AsEnumerable()
                where r.Field<int?>("place") != 999
                select r;
            if (StandardCorrectedTime > 0)
            {
                foreach (var dr in query)
                {
                    //
                    // Initially assume that achieved and new handicap are the current rolling handicap
                    //
                    dr["achieved_handicap"] = dr["open_handicap"];
                    var oldhc = (int) dr["rolling_handicap"];
                    if (dr["restricted_sail"] != DBNull.Value && (bool) dr["restricted_sail"])
                        dr["new_rolling_handicap"] = (int) Math.Round(oldhc/1.04);
                    else
                        dr["new_rolling_handicap"] = oldhc;

                    //
                    // we have a a standard corrected time for the race so we use this to work out
                    // the achieved performance.
                    //

                    //
                    // Acheived handicap is the ratio of boats corrected time according to the
                    // open handicap and the overall corrected time for the race multiplied by
                    // the open handicap.
                    //
                    var achhc =
                        (int)
                            Math.Round((double) dr["standard_corrected"]/StandardCorrectedTime*(int) dr["open_handicap"]);
                    dr["achieved_handicap"] = achhc;

                    //
                    // Next we look to see if the performance falls outside the +/- 5% band 
                    // (ie is an exceptional fast or slow result)
                    //
                    var sperf = false;
                    var sperfover = false;
                    if ((double) dr["standard_corrected"] >= SlowLimit || (double) dr["standard_corrected"] <= FastLimit)
                    {
                        if ((double) dr["standard_corrected"] >= SlowLimit)
                        {
                            dr["c"] = "s";
                            sperf = true;
                        }
                        else
                            dr["c"] = "F";

                        //
                        // This is a slow or fast performance (time outside the 5% above/below the average.
                        // If the previous perfomance was similarly slow then we will allow the
                        // handicap to change (if it would change).
                        //

                        var param = new Hashtable
                        {
                            ["bid"] = dr["bid"],
                            ["rid"] = Rid,
                            ["bstart"] = dr["start_date"]
                        };
                        var sl =
                            new Db(
                                @"SELECT TOP(1) CONVERT(FLOAT,(achieved_handicap - open_handicap))/open_handicap * 100
                            FROM races INNER JOIN calendar ON races.rid = calendar.rid
                            WHERE bid = @bid
                            AND races.rid != @rid
                            AND place != 999
                            AND standard_corrected_time <> 0
                            AND races.start_date <= @bstart
                            ORDER BY races.start_date DESC");
                        var slow = sl.GetData(param);

                        if (slow.Rows.Count >= 1)
                        {
                            //
                            // Found the last result prior to this one
                            //
                            var p1 = (double) slow.Rows[0][0];
                            if (p1 > 5)
                            {
                                //
                                // and it was also outside the 5% band (slow) so the handicap
                                // will be modified.
                                //
                                sperfover = true;
                                dr["c"] = "S";
                            }
                        }
                    }

                    dr["performance_index"] = (int) dr["achieved_handicap"] - (int) dr["open_handicap"];

                    //
                    // if this doesn't count as a slow race then adjust the handicap if the new value
                    // falls between +/- 5% of open.
                    //
                    if (!sperf || sperfover)
                    {
                        //
                        // See if the achieved handicap is outside the 5% above/below band
                        // and if it is use either +5% or -5% as the achieved handicap for 
                        // this calculation.  This is how we avoid letting the rolling handicap
                        // go outside the 5% band.
                        //
                        var working = achhc;
                        if (achhc > (int) dr["open_handicap"]*1.05)
                            working = (int) Math.Round(1.05*(int) dr["open_handicap"], 0);
                        if (achhc < (int) dr["open_handicap"]*0.95)
                            working = (int) Math.Round(0.95*(int) dr["open_handicap"], 0);

                        //
                        // Move the rolling handicap 15% towards the achieved handicap.
                        //
                        var newhc =
                            (int)
                                Math.Round((int) dr["rolling_handicap"] + (working - (int) dr["rolling_handicap"]) * Settings.RHCoefficieent);

                        //
                        // if rectricted sail was used then new handicap needs to be adjusted to remove the 4% increase
                        //
                        if (dr["restricted_sail"] != DBNull.Value && (bool) dr["restricted_sail"])
                            newhc = (int) Math.Round(newhc/1.04);
                        //
                        // And keep it if it's inside the band.
                        //
                        if (newhc >= (int) dr["open_handicap"]*0.95 && newhc <= (int) dr["open_handicap"]*1.05)
                            dr["new_rolling_handicap"] = newhc;
                    }
                }
            }

            p.Clear();
            var u = new Db(@"UPDATE boats
                    SET rolling_handicap = @new_rolling_handicap
                    WHERE bid = @bid
                    AND NOT EXISTS (SELECT 1
                    FROM races r1, races r2
                    WHERE r1.rid = @rid
                    AND r2.rid <> r1.rid
                    AND r2.bid = r1.bid
                    AND r1.bid = @bid
                    AND r2.start_date > r1.start_date)");

            p["rid"] = Rid;
            foreach (var dr in query)
            {
                p["new_rolling_handicap"] = dr["new_rolling_handicap"];
                p["bid"] = dr["bid"];
                u.ExecuteNonQuery(p);
            }
            u.Dispose();
        }
    }
}