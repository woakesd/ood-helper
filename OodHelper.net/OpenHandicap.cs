using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class OpenHandicap : RaceScore
    {
        private double standardCorrectedTime;
        private double averageCorrectedTime;
        private double slowLim;
        private double fastLim;
        private DateTime rdate;
        private string racetype;
        private int rid;

        public double SlowLimit
        {
            get { return slowLim; }
        }

        public double FastLimit
        {
            get { return fastLim; }
        }

        public void Calculate(int r)
        {
            rid = r;
            Hashtable p = new Hashtable();
            p["rid"] = rid;
            Db c = new Db(@"SELECT spec, date
                        FROM calendar
                        WHERE rid = @rid");
            Hashtable res = c.GetHashtable(p);
            rdate = (DateTime)res["date"];
            racetype = res["spec"].ToString();
            //handicap = res["hc"].ToString();
            CorrectedTime();
            CalculateSct();
            Score();
            UpdateHandicaps();
        }

        private void CorrectedTime()
        {
            //
            // update all boats setting elapsed, corrected, stdcorr, place and pts.
            //
            Hashtable p = new Hashtable();
            p["rid"] = rid;
            string sql = @"UPDATE races
                SET elapsed = 0
                ,corrected = 999999
                ,standard_corrected = 999999
                ,place = 999
                ,points = 0
                WHERE rid = @rid";
            Db c = new Db(sql);
            c.ExecuteNonQuery(p);
    
            //
            // Next select all boats and work out elapsed, corrected and stdcorr
            //
            sql = @"SELECT bid, rid, start, fintime, rolling_handicap, open_handicap, laps, 
                elapsed, corrected, standard_corrected, place
                FROM races
                WHERE rid = @rid";
            c = new Db(sql);
            DataTable dt = c.GetData(p);

            foreach (DataRow dr in dt.Rows)
            {
                if (dr["start"] != DBNull.Value && dr["fintime"] != DBNull.Value && dr["laps"] != DBNull.Value)
                {
                    string start = dr["start"].ToString();
                    string ftime = dr["fintime"].ToString();

                    Regex r1 = new Regex("[0-9][0-9].[0-9][0-9].[0-9][0-9]");
                    if (r1.Match(start).Success && r1.Match(ftime).Success)
                    {
                        
                        //
                        // So take start and finish as timespans and we can subtract one
                        // from the other to get the elapsed time.
                        //
                        TimeSpan s = Common.tspan(start).Value;
                        TimeSpan f = Common.tspan(ftime).Value;
                        
                        TimeSpan e = f - s;
                        dr["elapsed"] = e.TotalSeconds;

                        int l = (int) dr["laps"];
                        
                        int hcap = (int) dr["open_handicap"];

                        //
                        // if spec is 'a' then this is average lap so corrected times are per lap,
                        // otherwise assume everyone did same number of laps.
                        //
                        if (racetype == "a")
                        {
                            dr["corrected"] = Math.Round(e.TotalSeconds * 1000 / hcap / l);
                        }
                        else
                        {
                            dr["corrected"] = Math.Round(e.TotalSeconds * 1000 / hcap);
                        }
                        dr["standard_corrected"] = dr["corrected"];
                        dr["place"] = 0;
                    }
                }
            }
            //
            // Update the database.
            //
            c.Commit(dt);
            dt.Dispose();
        }

        private void CalculateSct()
        {
            //
            // First select the boats in the race that have Portsmouth, Secondary, Recorded or
            // Club numbers (handicaps).  These will be called good boats in the comments.
            //
            Db c = new Db(@"SELECT rid, bid, corrected, standard_corrected, a
                    FROM races
                    WHERE rid = @rid
                    AND handicap_status IN ('PY', 'SY', 'RN', 'CN')
                    AND place = 0
                    ORDER BY standard_corrected");
            Hashtable p = new Hashtable();
            p["rid"] = rid;
            DataTable d = c.GetData(p);

            //
            // if we have fewer than 2 good boats there is no
            // corrected time.
            //
            int qual = d.Rows.Count;
            if (qual < 2)
            {
                standardCorrectedTime = 0;
                return;
            }

            //
            // Loop through the top 2/3 of the good boats and work out their 
            // average corrected time.
            //
            double total = 0;
            int n = (int) Math.Round(qual * 0.67);

            for (int i = 0; i < n; i++)
            {
                total = total + (double) d.Rows[i]["standard_corrected"];
            }

            averageCorrectedTime = total / n;

            //
            // The average slow limit is the average corrected time + 5%.
            //
            double AvgSlowLimit = averageCorrectedTime * 1.05;

            //
            // Next count the number of good boats that beat the slow time.
            // Also sum their standard corrected times.
            //
            int goodBoats = 0;
            standardCorrectedTime = 0;
            for (int i = 0; i < d.Rows.Count; i++)
            {
                DataRow row = d.Rows[i];
                row["a"] = "N";
                if (((double) row["standard_corrected"]) < AvgSlowLimit)
                {
                    row["a"] = DBNull.Value;
                    goodBoats++;
                    standardCorrectedTime = standardCorrectedTime + (double)row["standard_corrected"];
                }           
            }

            if (goodBoats > 1)
            {
                //
                // More than 1 good boat so we have enough to calculate the SCT
                // which is the average standard corrected time of the good boats
                // that beat the 5% slow cutoff.
                //
                standardCorrectedTime = Math.Round(standardCorrectedTime / goodBoats);

                //
                // NB These limits are related to the standard corrected time and
                // are the ones used to when working out new handicaps.
                //
                slowLim = Math.Round(standardCorrectedTime * 1.05);
                fastLim = Math.Round(standardCorrectedTime * 0.95);
            }
            else
            {
                //
                // Too few good boats so we cant work this out.
                //
                standardCorrectedTime = 0;
                slowLim = 0;
                fastLim = 0;
            }
            Hashtable param = new Hashtable();
            param["rid"] = rid;
            param["sct"] = standardCorrectedTime;

            Db up = new Db(@"UPDATE calendar SET standard_corrected_time = @sct, raced = 1 WHERE rid = @rid");
            up.ExecuteNonQuery(param);
            
            c.Commit(d);
            d.Dispose();
        }

        private void Score()
        {
            //
            // This routine sets the places column and the pts column.
            //

            Db c = new Db(@"SELECT bid, rid, corrected, place, points
                    FROM races
                    WHERE rid = @rid
                    AND place = 0
                    ORDER BY corrected");
            Hashtable p = new Hashtable();
            p["rid"] = rid;
            DataTable d = c.GetData(p);

            //
            // First give everyone a placing.
            //
            for (int i = 0; i < d.Rows.Count; i++)
            {
                //
                // NB tied boats are given the same place, so if two boats are tied in 3rd
                // they both are assigned 3rd.  The boat that follows them is still in 5th.
                //
                if (i == 0)
                    //
                    // first row, so set place.
                    //
                    d.Rows[i]["place"] = 1;
                else
                {
                    //
                    // Subsequent rows with non zero elapsed time.
                    //
                    if (d.Rows[i - 1]["corrected"].ToString() == d.Rows[i]["corrected"].ToString())
                        //
                        // corrected time is the same as the previous row, so use place
                        // from previous row as we are tied.
                        //
                        d.Rows[i]["place"] = d.Rows[i - 1]["place"];
                    else
                        //
                        // Not tied so assign place.
                        //
                        d.Rows[i]["place"] = i + 1;
                }
            }

            //
            // Next we must assign points which are normally the same as the place unless there is a tie
            // in which case the points are shared.  So two boats tied in 3rd will share the points for
            // 3th and 4th and get 3.5 points each.  THIS IS AS PER RRS.
            //
            // If 3 boats tie in 3rd the points are the average of 3, 4 and 5 which is 4, and so on.
            //
            int j = 0;
            while (j < d.Rows.Count)
            {
                int k = j + 1;
                double psum = k;
                while (k < d.Rows.Count)
                {
                    if (d.Rows[j]["place"].ToString() != d.Rows[k]["place"].ToString())
                        break;
                    k++;
                    psum = psum + k;
                }

                double avg = Math.Round(psum / (k - j), 2);
                for (int l = j; l < k; l++)
                    d.Rows[l]["points"] = avg;
                j = k;
            }

            c.Commit(d);
            d.Dispose();
        }

        private void UpdateHandicaps()
        {
            //
            // This routine sets the places column and the pts column.
            //

            Db c = new Db(@"SELECT bid, rid, start, standard_corrected, rolling_handicap, open_handicap, 
                    achieved_handicap, new_rolling_handicap, c
                    FROM races
                    WHERE rid = @rid
                    AND place <> 999
                    ORDER BY corrected");
            Hashtable p = new Hashtable();
            p["rid"] = rid;
            DataTable d = c.GetData(p);
           
            foreach (DataRow dr in d.Rows)
            {
                //
                // Initially assume that achieved and new handicap are the current rolling handicap
                //
                int bid = (int)dr["bid"];
                dr["achieved_handicap"] = dr["rolling_handicap"];
                dr["new_rolling_handicap"] = dr["rolling_handicap"];
                if (standardCorrectedTime > 0)
                {
                    //
                    // we have a a standard corrected time for the race so we use this to work out
                    // the achieved performance.
                    //

                    //
                    // Acheived handicap is the ratio of boats corrected time according to the
                    // open handicap and the overall corrected time for the race multiplied by
                    // the open handicap.
                    //
                    int achhc = (int) Math.Round((double)dr["standard_corrected"] / standardCorrectedTime * (int) dr["open_handicap"]);
                    dr["achieved_handicap"] = achhc;

                    //
                    // Next we look to see if the performance falls outside the +/- 5% band 
                    // (ie is an exceptional fast or slow result)
                    //
                    bool sperf = false;
                    bool sperfover = false;
                    if ((double)dr["standard_corrected"] > SlowLimit || (double)dr["standard_corrected"] < FastLimit)
                    {
                        if ((double)dr["standard_corrected"] > SlowLimit)
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
                        Hashtable param = new Hashtable();
                        param["bid"] = dr["bid"];
                        param["rid"] = rid;
                        TimeSpan bstart = Common.tspan(dr["start"].ToString()).Value;
                        DateTime bdate = rdate + bstart;
                        param["bstart"] = bdate;
                        Db sl = new Db(@"SELECT TOP(1) CONVERT(FLOAT,(achieved_handicap - open_handicap))/open_handicap * 100
                            FROM races
                            WHERE bid = @bid
                            AND rid != @rid
                            AND place != 999
                            AND CONVERT(DATETIME,SUBSTRING(CONVERT(NCHAR(19),date),1,11) + ' ' + replace(start,' ',':'))
                                <= @bstart
                            ORDER BY date DESC, start DESC");
                        DataTable slow = sl.GetData(param);

                        if (slow.Rows.Count >= 1)
                        {
                            //
                            // Found the last result prior to this one
                            //
                            double p1 = (double) slow.Rows[0][0];
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
                        int working = achhc;
                        if (achhc > (int) dr["open_handicap"] * 1.05)
                            working = (int)Math.Round(1.05 * (int)dr["open_handicap"], 0);
                        if (achhc < (int)dr["open_handicap"] * 0.95)
                            working = (int)Math.Round(0.95 * (int)dr["open_handicap"], 0);

                        //
                        // Move the rolling handicap 15% towards the achieved handicap.
                        //
                        int newhc = (int)Math.Round((int)dr["rolling_handicap"] + (working - (int)dr["rolling_handicap"]) * 0.15);

                        //
                        // And keep it if it's inside the band.
                        //
                        if (newhc >= (int)dr["rolling_handicap"] * 0.95 && newhc <= (int)dr["rolling_handicap"] * 1.05)
                            dr["new_rolling_handicap"] = newhc;
                    }
                }
            }

            c.Commit(d);
            d.Dispose();

            p = new Hashtable();
            p["rid"] = rid;

            c = new Db(@"SELECT bid, new_rolling_handicap
                    FROM races
                    WHERE rid = @rid");
            d = c.GetData(p);

            Db u = new Db(@"SELECT bid, rolling_handicap
                    FROM boats
                    WHERE boats.bid IN (SELECT races.bid
                    FROM races
                    WHERE races.rid = @rid)");
            DataTable ut = u.GetData(p);

            foreach (DataRow dr in ut.Rows)
            {
                DataRow[] rows = d.Select("bid = " + dr["bid"].ToString());
                dr["rolling_handicap"] = rows[0]["new_rolling_handicap"];
            }
            ut.Dispose();

            c.Commit(d);
            d.Dispose();
        }
    }
}
