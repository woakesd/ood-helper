using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data.SqlServerCe;

namespace OodHelper.net
{
    [Svn("$Id: Common.cs 17588 2010-05-04 19:37:30Z david $")]
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

        public static void copyMySqlData()
        {
            Properties.Settings s = new Properties.Settings();
            MySqlConnection mcon = new MySqlConnection(s.mysql);
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

            scmd.CommandText = "SET IDENTITY_INSERT boats ON";
            scmd.ExecuteNonQuery();
            
            SqlCeCommand ins = new SqlCeCommand("INSERT INTO [boats] ([boatname], [boatclass], [sailno], [dngy], [h], [bid], [distance], [crewname], [ohp], [ohstat], [chp], [rhp], [ihp], [csf], [eng], [kl], [deviations], [subscriptn], [p], [s], [id], [beaten], [berth], [boatmemo], [hired]) " +
                "VALUES (@boatname, @boatclass, @sailno, @dngy, @h, @bid, @distance, @crewname, @ohp, @ohstat, @chp, @rhp, @ihp, @csf, @eng, @kl, @deviations, @subscriptn, @p, @s, @id, @beaten, @berth, @boatmemo, @hired)"
                , scon);
            
            copyData(mtable, ins);

            scmd.CommandText = "SET IDENTITY_INSERT boats OFF";
            scmd.ExecuteNonQuery();

            //
            // Events
            //
            ins.CommandText = "INSERT INTO [calendar] ([rid], [date], [dow], [class], [event], [start], [gp], [course], [ood], [venue], [spec], [hc], [hc_ul], [hc_meth], [vis], [flag], [timelimit], [extension], [computer], [memo], [r], [app], [p], [sct], [check]) " +
                "VALUES (@rid, @date, @dow, @class, @event, @start, @gp, @course, @ood, @venue, @spec, @hc, @hc_ul, @hc_meth, @vis, @flag, @timelimit, @extension, @computer, @memo, @r, @app, @p, @sct, @check)";

            myadp = new MySqlDataAdapter("SELECT * FROM calendar", mcon);
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
            ins.CommandText = "INSERT INTO [PEOPLE] ([id], [firstname], [surname], [address1], [address2], [address3], [address4], [postcode], [hometel], [worktel], [mobile], [email], [club], [member], [cp], [s], [sid], [check], [manmemo], [novice]) " +
                "VALUES (@id, @firstname, @surname, @address1, @address2, @address3, @address4, @postcode, @hometel, @worktel, @mobile, @email, @club, @member, @cp, @s, @sid, @check, @manmemo, @novice)";

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
            ins.CommandText = "INSERT INTO [races] ([rid], [date], [bid], [fincode], [fintime], [findate], [laps], [elapsed], [corrected], [hcap], [place], [stdcorr], [ohstat], [a], [achhc], [prfdx], [newhc], [c], [start], [bstart], [pts], [ov_pts], [ohp], [ihp], [check]) " +
                "VALUES (@rid, @date, @bid, @fincode, @fintime, @findate, @laps, @elapsed, @corrected, @hcap, @place, @stdcorr, @ohstat, @a, @achhc, @prfdx, @newhc, @c, @start, @bstart, @pts, @ov_pts, @ohp, @ihp, @check)";
            myadp = new MySqlDataAdapter("SELECT * FROM races", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            scmd.CommandText = "DELETE FROM races";
            scmd.ExecuteNonQuery();

            copyData(mtable, ins);

            //
            // Series
            //
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

    }
}
