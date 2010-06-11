using System;
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
                ins.CommandText = "INSERT INTO [calendar] ([rid], [start_date], [class], [event], [price_code], [course], [ood], [venue], [average_lap], [timegate], [handicapping], [visitors], [flag], [time_limit_type], [time_limit_fixed], [extension], [memo], [raced], [approved], [standard_corrected_time]) " +
                    "VALUES (@rid, @start_date, @class, @event, @gp, @course, @ood, @venue, @avg, @tgate, @hc, @vis, @flag, 'F', @time_limit_fixed, @extension, @memo, @r, @app, @sct)";

                myadp = new MySqlDataAdapter("SELECT rid, " +
                    "str_to_date(concat(date_format(date,'%Y-%m-%d '), case when start = ':' then '00:00' else start end),'%Y-%m-%d %H:%i') start_date, " +
                    "case when timelimit is not null then str_to_date(concat(date_format(date,'%Y-%m-%d '), timelimit),'%Y-%m-%d %H:%i') else null end time_limit_fixed, " +
                    "class, event, gp, course, ood, venue, " +
                    "case when spec = 'a' then 1 else 0 end as avg, case when spec = 't' then 1 else 0 end as tgate, " +
                    "hc, vis, flag, time_to_sec(str_to_date(extension, '%H:%i')) extension, memo, r, app, sct FROM calendar", mcon);
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
                ins.CommandText = "INSERT INTO [races] ([rid], [bid], [date], [start], [fincode], [fintime], [findate], [laps], [elapsed], [corrected], [standard_corrected], [handicap_status], [open_handicap], [rolling_handicap], [achieved_handicap], [new_rolling_handicap], [place], [points], [override_points], [performance_index], [a], [c]) " +
                    "VALUES (@rid, @bid, @date, @bstart, @fincode, @fintime, @findate, @laps, @elapsed, @corrected, @stdcorr, @ohstat, @ohp, @hcap, @achhc, @newhc, @place, @pts, @ov_pts, @prfdx, @a, @c)";
                myadp = new MySqlDataAdapter("SELECT * FROM races", mcon);
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
                return "";
        }
    }
}
