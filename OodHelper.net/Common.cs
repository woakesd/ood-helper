using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data.SqlServerCe;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class Common
    {
        public static TimeSpan? tspan(String s)
        {
            TimeSpan t;
            try
            {
                if (s.Length == 5)
                {
                    t = new TimeSpan(int.Parse(s.Substring(0, 2)),
                        int.Parse(s.Substring(3, 2)), 0);
                }
                else
                {
                    t = new TimeSpan(int.Parse(s.Substring(0, 2)),
                        int.Parse(s.Substring(3, 2)),
                        int.Parse(s.Substring(6, 2)));
                }
            }
            catch
            {
                return null;
            }
            return t;
        }

        public static void copyToMySql(Working w)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                w.SetProgress("Uploading boats", 0);
                string mysql = (string)DbSettings.GetSetting("mysql");
                MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(mysql);
                mysql = mcsb.ConnectionString;
                MySqlConnection mcon = new MySqlConnection(mysql);
                mcon.Open();

                MySqlCommand mcom = new MySqlCommand("DELETE FROM boats");
                mcom.Connection = mcon;
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `boats` DISABLE KEYS";
                mcom.ExecuteNonQuery();

                StringBuilder msql = new StringBuilder("INSERT INTO `boats` (`boatname`,`boatclass`,`sailno`,`dngy`,`h`,`bid`,");
                msql.Append("`distance`,`crewname`,`ohp`,`ohstat`,`chp`,`rhp`,`ihp`,`csf`,`eng`,`kl`,`deviations`,`subscriptn`,`p`,`s`,");
                msql.Append("`boatmemo`,`id`,`beaten`,`berth`,`hired`) VALUES ");

                Db c = new Db(@"SELECT boatname, boatclass, sailno, dinghy dngy, hulltype h, bid,
                    distance, crewname, open_handicap ohp, handicap_status ohstat, null chp, rolling_handicap rhp, null ihp, crew_skill_factor csf, engine_propeller eng, keel kl,
                    deviations, subscription subscriptn, p, s, boatmemo, id, beaten, berth, hired
                    FROM boats");
                DataTable d = c.GetData(null);
                c.Dispose();

                BuildInsertData(d, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `boats` ENABLE KEYS";
                mcom.ExecuteNonQuery();

                w.SetProgress("Uploading calendar", 1);

                mcom.CommandText = "DELETE FROM calendar_new";
                mcom.Connection = mcon;
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `calendar_new` DISABLE KEYS";
                mcom.ExecuteNonQuery();

                msql.Clear();
                msql.Append("INSERT INTO `calendar_new` (`rid`,`start_date`,`time_limit_type`,`time_limit_fixed`,");
                msql.Append("`time_limit_delta`,`extension`,`class`,`event`,`price_code`,`course`,`ood`,`venue`,`average_lap`,");
                msql.Append("`timegate`,`handicapping`,`visitors`,`flag`,`memo`,`is_race`,`raced`,`approved`,`course_choice`,");
                msql.Append("`laps_completed`,`wind_speed`,`wind_direction`,`standard_corrected_time`,`result_calculated`) VALUES ");

                c = new Db(@"SELECT rid, start_date, time_limit_type, time_limit_fixed,
                        time_limit_delta, extension, class, event, price_code, course, ood, venue, average_lap,
                        timegate, handicapping, visitors, flag, memo, is_race, raced, approved, course_choice,
                        laps_completed, wind_speed, wind_direction, standard_corrected_time, result_calculated
                        FROM calendar");
                d = c.GetData(null);
                c.Dispose();

                BuildInsertData(d, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `calendar_new` ENABLE KEYS";
                mcom.ExecuteNonQuery();

                w.SetProgress("Uploading people", 2);

                mcom.CommandText = "DELETE FROM `people`";
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `people` DISABLE KEYS";
                mcom.ExecuteNonQuery();

                msql.Clear();
                msql.Append("INSERT INTO `people` (`firstname`,`surname`,`address1`,`address2`,`address3`,`address4`,`postcode`,");
                msql.Append("`hometel`,`worktel`,`mobile`,`email`,`club`,`member`,`cp`,`s`,`id`,`manmemo`,`sid`,`novice`) VALUES ");

                c = new Db(@"SELECT firstname,surname,address1,address2,address3,address4,postcode,hometel,worktel,mobile,email,
                        club,member,cp,s,id,manmemo,main_id sid,novice
                        FROM people");
                d = c.GetData(null);

                BuildInsertData(d, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `people` ENABLE KEYS";
                mcom.ExecuteNonQuery();

                w.SetProgress("Uploading races", 3);

                mcom.CommandText = "DELETE FROM `races_new`";
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `races_new` DISABLE KEYS";
                mcom.ExecuteNonQuery();

                msql.Clear();
                msql.Append("INSERT INTO `races_new` (rid,bid,start_date,finish_code,finish_date,last_edit,laps,place,points,override_points,");
                msql.Append("elapsed,corrected,standard_corrected,handicap_status,open_handicap,rolling_handicap,achieved_handicap,");
                msql.Append("new_rolling_handicap,performance_index,a,c) VALUES ");

                c = new Db(@"SELECT rid,bid,start_date,finish_code,finish_date,last_edit,laps,place,points,override_points,
                        elapsed,corrected,standard_corrected,handicap_status,open_handicap,rolling_handicap,achieved_handicap,
                        new_rolling_handicap,performance_index,a,c
                        FROM races");
                d = c.GetData(null);

                BuildInsertData(d, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `races_new` ENABLE KEYS";
                mcom.ExecuteNonQuery();

                w.SetProgress("Uploading series", 4);

                mcom.CommandText = "DELETE FROM `series`";
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `series` DISABLE KEYS";
                mcom.ExecuteNonQuery();

                msql.Clear();
                msql.Append("INSERT INTO `series` (sid,sname) VALUES ");

                c = new Db(@"SELECT sid,sname FROM series");
                d = c.GetData(null);

                BuildInsertData(d, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `series` ENABLE KEYS";
                mcom.ExecuteNonQuery();

                w.SetProgress("Uploading calendar series join", 5);

                mcom.CommandText = "DELETE FROM `calendar_series_join`";
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `calendar_series_join` DISABLE KEYS";
                mcom.ExecuteNonQuery();

                msql.Clear();
                msql.Append("INSERT INTO `calendar_series_join` (sid,rid) VALUES ");

                c = new Db(@"SELECT sid,rid FROM calendar_series_join");
                d = c.GetData(null);

                BuildInsertData(d, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `calendar_series_join` ENABLE KEYS";
                mcom.ExecuteNonQuery();

                w.SetProgress("All done", 6);
                w.CloseWindow();
            });
        }

        public static void BuildInsertData(DataTable d, StringBuilder msql)
        {
            for (int i = 0; i < d.Rows.Count; i++)
            {
                DataRow dr = d.Rows[i];

                msql.Append("(");
                for (int j = 0; j < d.Columns.Count; j++)
                {
                    switch (d.Columns[j].DataType.ToString())
                    {
                        case "System.String":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("'{0}'", dr[j].ToString().Replace("'", "''"));
                            else
                                msql.Append("NULL");
                            break;
                        case "System.Int32":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("{0}", dr[j]);
                            else
                                msql.Append("NULL");
                            break;
                        case "System.Double":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("{0}", dr[j]);
                            else
                                msql.Append("NULL");
                            break;
                        case "System.DateTime":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("'{0:yyyy-MM-dd HH:mm:ss}'", dr[j]);
                            else
                                msql.Append("NULL");
                            break;
                        case "System.Boolean":
                            if (dr[j] == DBNull.Value)
                                msql.Append("NULL");
                            else if ((bool)dr[j])
                                msql.Append("true");
                            else
                                msql.Append("false");
                            break;
                    }
                    if (j < d.Columns.Count - 1) msql.Append(",");
                }
                msql.Append(")");
                if (i < d.Rows.Count - 1) msql.Append(",");
            }
        }

        public static void copyMySqlData(Working p)
        {
            p.SetProgress("Loading Boats", 0);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                string mysql = (string)DbSettings.GetSetting("mysql");
                MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(mysql);
                mysql = mcsb.ConnectionString;
                MySqlConnection mcon = new MySqlConnection(mysql);
                mcon.Open();

                System.Data.SqlServerCe.SqlCeConnection scon =
                    new System.Data.SqlServerCe.SqlCeConnection();
                scon.ConnectionString = Properties.Settings.Default.OodHelperConnectionString;
                scon.Open();

                //
                // Boats
                //
                MySqlDataAdapter myadp = new MySqlDataAdapter("SELECT * FROM boats", mcon);
                DataTable mtable = new DataTable();
                myadp.Fill(mtable);

                SqlCeCommand scmd = new SqlCeCommand();
                scmd.Connection = scon;

                scmd.CommandText = "DELETE FROM boats";
                scmd.ExecuteNonQuery();

                SqlCeCommand ins = new SqlCeCommand("INSERT INTO [boats] " +
                    "([bid], [id], [boatname], [boatclass], [sailno], [dinghy], [hulltype], [distance], [crewname], [open_handicap], [handicap_status], [rolling_handicap], [crew_skill_factor], [engine_propeller], [keel], [deviations], [subscription], [berth], [boatmemo], [hired], [p], [s], [beaten]) " +
                    "VALUES " +
                    "(@bid, @id, @boatname, @boatclass, @sailno, @dngy, @h, @distance, @crewname, @ohp, @ohstat, @rhp, @csf, @eng, @kl, @deviations, @subscriptn, @berth, @boatmemo, @hired, @p, @s, @beaten)"
                    , scon);

                scmd.CommandText = "SET IDENTITY_INSERT boats ON";
                scmd.ExecuteNonQuery();

                copyData(mtable, ins);

                scmd.CommandText = "SET IDENTITY_INSERT boats OFF";
                scmd.ExecuteNonQuery();

                //
                // Events
                //
                p.SetProgress("Loading Calendar", 1);
                ins.CommandText = "INSERT INTO [calendar] ([rid], [start_date], [class], [event], [price_code], [course], [ood], [venue], [average_lap], [timegate], [handicapping], [visitors], [flag], [time_limit_type], [time_limit_fixed], [extension], [memo], [is_race], [raced], [approved], [course_choice], [laps_completed], [wind_speed], [wind_direction], [standard_corrected_time], [result_calculated]) " +
                    "VALUES (@rid, @start_date, @class, @event, @price_code, @course, @ood, @venue, @average_lap, @timegate, @handicapping, @visitors, @flag, @time_limit_type, @time_limit_fixed, @extension, @memo, @is_race, @raced, @approved, @course_choice, @laps_completed, @wind_speed, @wind_direction, @standard_corrected_time, @result_calculated)";

                myadp = new MySqlDataAdapter("SELECT `rid`, `start_date`, `class`, `event`, `price_code`, `course`, `ood`, `venue`, `average_lap`, `timegate`, `handicapping`, `visitors`, `flag`, `time_limit_type`, `time_limit_fixed`, `extension`, `memo`, `is_race`, `raced`, `approved`, `course_choice`, `laps_completed`, `wind_speed`, `wind_direction`, `standard_corrected_time`, `result_calculated` FROM calendar_new", mcon);
                mtable = new DataTable();
                myadp.Fill(mtable);

                scmd.CommandText = "DELETE FROM calendar";
                scmd.ExecuteNonQuery();

                scmd.CommandText = "SET IDENTITY_INSERT calendar ON";
                scmd.ExecuteNonQuery();

                copyData(mtable, ins);

                scmd.CommandText = "SET IDENTITY_INSERT calendar OFF";
                scmd.ExecuteNonQuery();

                //
                // People
                //
                p.SetProgress("Loading People", 2);
                ins.CommandText = "INSERT INTO [PEOPLE] ([id], [main_id], [firstname], [surname], [address1], [address2], [address3], [address4], [postcode], [hometel], [worktel], [mobile], [email], [club], [member], [cp], [s], [manmemo], [novice]) " +
                    "VALUES (@id, @sid, @firstname, @surname, @address1, @address2, @address3, @address4, @postcode, @hometel, @worktel, @mobile, @email, @club, @member, @cp, @s, @manmemo, @novice)";

                myadp = new MySqlDataAdapter("SELECT * FROM people", mcon);
                mtable = new DataTable();
                myadp.Fill(mtable);

                scmd.CommandText = "DELETE FROM people";
                scmd.ExecuteNonQuery();

                scmd.CommandText = "SET IDENTITY_INSERT people ON";
                scmd.ExecuteNonQuery();

                copyData(mtable, ins);

                scmd.CommandText = "SET IDENTITY_INSERT people OFF";
                scmd.ExecuteNonQuery();

                //
                // Races
                //
                p.SetProgress("Loading Races", 3);
                ins.CommandText = "INSERT INTO [races] ([rid], [bid], [start_date], [finish_date], [finish_code], [laps], [elapsed], [corrected], [standard_corrected], [handicap_status], [open_handicap], [rolling_handicap], [achieved_handicap], [new_rolling_handicap], [place], [points], [override_points], [performance_index], [a], [c]) " +
                    "VALUES (@rid, @bid, @start_date, @finish_date, @finish_code, @laps, @elapsed, @corrected, @standard_corrected, @handicap_status, @open_handicap, @rolling_handicap, @achieved_handicap, @new_rolling_handicap, @place, @points, @override_points, @performance_index, @a, @c)";

                myadp = new MySqlDataAdapter("SELECT `rid`, `bid`, `start_date`, `finish_date`, `finish_code`, `laps`, `elapsed`, `corrected`, `standard_corrected`, `handicap_status`, `open_handicap`, `rolling_handicap`, `achieved_handicap`, `new_rolling_handicap`, `place`, `points`, `override_points`, `performance_index`, `a`, `c` FROM races_new", mcon);
                mtable = new DataTable();
                myadp.Fill(mtable);

                scmd.CommandText = "DELETE FROM races";
                scmd.ExecuteNonQuery();

                copyData(mtable, ins);

                //
                // Series
                //
                p.SetProgress("Loading Series", 4);
                ins.CommandText = "INSERT INTO [series] ([sid], [sname]) " +
                    "VALUES (@sid, @sname)";
                myadp = new MySqlDataAdapter("SELECT * FROM series", mcon);
                mtable = new DataTable();
                myadp.Fill(mtable);

                scmd.CommandText = "DELETE FROM series";
                scmd.ExecuteNonQuery();

                scmd.CommandText = "SET IDENTITY_INSERT series ON";
                scmd.ExecuteNonQuery();

                copyData(mtable, ins);

                scmd.CommandText = "SET IDENTITY_INSERT series OFF";
                scmd.ExecuteNonQuery();

                //
                // Calendar Series Link
                //
                p.SetProgress("Loading Series links", 5);
                ins.CommandText = "INSERT INTO [calendar_series_join] ([sid], [rid]) " +
                    "VALUES (@sid, @rid)";
                myadp = new MySqlDataAdapter("SELECT * FROM calendar_series_join", mcon);
                mtable = new DataTable();
                myadp.Fill(mtable);

                scmd.CommandText = "DELETE FROM calendar_series_join";
                scmd.ExecuteNonQuery();

                copyData(mtable, ins);

                scmd.CommandText = @"SELECT r1.rid rid, r1.bid bid, 
                        CASE WHEN r2.new_rolling_handicap IS NOT NULL THEN r2.new_rolling_handicap ELSE b.rolling_handicap END rolling_handicap
                        FROM races r1
                        INNER JOIN calendar c ON r1.rid = c.rid
                        INNER JOIN boats b ON r1.bid = b.bid
                        LEFT JOIN races r2 ON r2.rid <> r1.rid
                        AND r2.bid = r1.bid
                        AND r2.start_date < r1.start_date
                        AND r2.start_date IN (SELECT MAX(r3.start_date)
                        FROM races r3
                        WHERE r3.start_date < r1.start_date
                        AND r3.bid = r1.bid
                        AND r3.rid <> r1.rid)
                        WHERE c.handicapping = 'o'";

                SqlCeDataAdapter sda = new SqlCeDataAdapter(scmd);
                DataTable d = new DataTable();
                sda.Fill(d);
                foreach (DataRow dr in d.Rows)
                {
                    scmd.CommandText = @"UPDATE races
                            SET rolling_handicap = @rolling_handicap
                            WHERE rid = @rid
                            AND bid = @bid";
                    scmd.Parameters.Clear();
                    scmd.Parameters.Add("rid", dr["rid"]);
                    scmd.Parameters.Add("bid", dr["bid"]);
                    scmd.Parameters.Add("rolling_handicap", dr["rolling_handicap"]);
                    scmd.ExecuteNonQuery();
                }

                scon.Close();
                mcon.Close();

                Db.Compact();
                p.SetProgress("All done", 6);
                p.CloseWindow();
            });
        }

        private static void copyData(DataTable rset, SqlCeCommand ins)
        {
            foreach (DataRow rrow in rset.Rows)
            {
                foreach (DataColumn rc in rset.Columns)
                {
                    ins.Parameters.Add(rc.ColumnName, rrow[rc.ColumnName]);
                }
                SqlCeParameter p1 = ins.Parameters[1];
                ins.ExecuteNonQuery();
                ins.Parameters.Clear();
            }
        }

        public static string HMS(double t)
        {
            if (t != 999999)
            {
                int s = (int)t % 60;
                int m = (int)t / 60;
                int h = m / 60;
                m = m % 60;
                return h.ToString().PadLeft(2, '0') + ':' +
                    m.ToString().PadLeft(2, '0') + ':' +
                    s.ToString().PadLeft(2, '0');
            }
            else
                return string.Empty;
        }
    }
}
