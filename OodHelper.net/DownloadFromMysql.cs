using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data.SqlServerCe;
using System.ComponentModel;
using System.Windows;

namespace OodHelper.net
{
    class DownloadFromMysql
    {
        public DownloadFromMysql()
        {
            BackgroundWorker download = new BackgroundWorker();
            download.DoWork += new DoWorkEventHandler(Process);
            Working w = new Working(Application.Current.MainWindow, download);
            download.RunWorkerCompleted += new RunWorkerCompletedEventHandler(download_RunWorkerCompleted);
            download.RunWorkerAsync();
            w.ShowDialog();
        }
        
        private SqlCeConnection scon;
        private SqlCeTransaction strn;

        public void download_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                System.Windows.MessageBox.Show("Download Cancelled", "Cancel", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            else
                System.Windows.MessageBox.Show("Download Complete", "Finished", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
        }

        private void CancelDownload(DoWorkEventArgs e)
        {
            e.Cancel = true;
            strn.Rollback();
            strn.Dispose();
            scon.Close();
            scon.Dispose();
            return;
        }

        public void Process(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker p = sender as BackgroundWorker;

            scon = new SqlCeConnection();
            scon.ConnectionString = Properties.Settings.Default.OodHelperConnectionString;
            scon.Open();
            strn = scon.BeginTransaction();

            string mysql = (string)DbSettings.GetSetting("mysql");
            MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(mysql);
            mysql = mcsb.ConnectionString;
            MySqlConnection mcon = new MySqlConnection(mysql);
            mcon.Open();

            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress(0, "Loading Boats");

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

            CopyData(mtable, ins);

            scmd.CommandText = "SET IDENTITY_INSERT boats OFF";
            scmd.ExecuteNonQuery();

            //
            // Events
            //
            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress(100 / 6, "Loading Calendar");
            ins.CommandText = "INSERT INTO [calendar] ([rid], [start_date], [class], [event], [price_code], [course], [ood], " +
                "[venue], [average_lap], [timegate], [handicapping], [visitors], [flag], [time_limit_type], [time_limit_fixed], " +
                "[time_limit_delta], [extension], [memo], [is_race], [raced], [approved], [course_choice], [laps_completed], " +
                "[wind_speed], [wind_direction], [standard_corrected_time], [result_calculated]) " +
                "VALUES (@rid, @start_date, @class, @event, @price_code, @course, @ood, @venue, @average_lap, @timegate, " +
                "@handicapping, @visitors, @flag, @time_limit_type, @time_limit_fixed, @time_limit_delta, @extension, @memo, " +
                "@is_race, @raced, @approved, @course_choice, @laps_completed, @wind_speed, @wind_direction, " +
                "@standard_corrected_time, @result_calculated)";

            myadp = new MySqlDataAdapter("SELECT `rid`, `start_date`, `class`, `event`, `price_code`, `course`, `ood`, `venue`, " +
                "`average_lap`, `timegate`, `handicapping`, `visitors`, `flag`, `time_limit_type`, `time_limit_fixed`, " +
                "`time_limit_delta`, `extension`, `memo`, `is_race`, `raced`, `approved`, `course_choice`, `laps_completed`, " +
                "`wind_speed`, `wind_direction`, `standard_corrected_time`, `result_calculated` FROM calendar_new", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            scmd.CommandText = "DELETE FROM calendar";
            scmd.ExecuteNonQuery();

            scmd.CommandText = "SET IDENTITY_INSERT calendar ON";
            scmd.ExecuteNonQuery();

            CopyData(mtable, ins);

            scmd.CommandText = "SET IDENTITY_INSERT calendar OFF";
            scmd.ExecuteNonQuery();

            //
            // People
            //
            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress(200 / 6, "Loading People");
            ins.CommandText = "INSERT INTO [PEOPLE] ([id], [main_id], [firstname], [surname], [address1], [address2], [address3], [address4], [postcode], [hometel], [worktel], [mobile], [email], [club], [member], [cp], [s], [manmemo], [novice]) " +
                "VALUES (@id, @sid, @firstname, @surname, @address1, @address2, @address3, @address4, @postcode, @hometel, @worktel, @mobile, @email, @club, @member, @cp, @s, @manmemo, @novice)";

            myadp = new MySqlDataAdapter("SELECT * FROM people", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            scmd.CommandText = "DELETE FROM people";
            scmd.ExecuteNonQuery();

            scmd.CommandText = "SET IDENTITY_INSERT people ON";
            scmd.ExecuteNonQuery();

            CopyData(mtable, ins);

            scmd.CommandText = "SET IDENTITY_INSERT people OFF";
            scmd.ExecuteNonQuery();

            //
            // Races
            //
            p.ReportProgress(300 / 6, "Loading Races");
            ins.CommandText = "INSERT INTO [races] ([rid], [bid], [start_date], [finish_date], [last_edit], [finish_code], [laps], [elapsed], [corrected], [standard_corrected], [handicap_status], [open_handicap], [rolling_handicap], [achieved_handicap], [new_rolling_handicap], [place], [points], [override_points], [performance_index], [a], [c]) " +
                "VALUES (@rid, @bid, @start_date, @finish_date, @last_edit, @finish_code, @laps, @elapsed, @corrected, @standard_corrected, @handicap_status, @open_handicap, @rolling_handicap, @achieved_handicap, @new_rolling_handicap, @place, @points, @override_points, @performance_index, @a, @c)";

            myadp = new MySqlDataAdapter("SELECT `rid`, `bid`, `start_date`, `finish_date`, `last_edit`, `finish_code`, `laps`, `elapsed`, `corrected`, `standard_corrected`, `handicap_status`, `open_handicap`, `rolling_handicap`, `achieved_handicap`, `new_rolling_handicap`, `place`, `points`, `override_points`, `performance_index`, `a`, `c` FROM races_new", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            scmd.CommandText = "DELETE FROM races";
            scmd.ExecuteNonQuery();

            CopyData(mtable, ins);

            //
            // Series
            //
            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress(400 / 6, "Loading Series");
            ins.CommandText = "INSERT INTO [series] ([sid], [sname], [discards]) " +
                "VALUES (@sid, @sname, @discards)";
            myadp = new MySqlDataAdapter("SELECT sid, sname, discards FROM series", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            scmd.CommandText = "DELETE FROM series";
            scmd.ExecuteNonQuery();

            scmd.CommandText = "SET IDENTITY_INSERT series ON";
            scmd.ExecuteNonQuery();

            CopyData(mtable, ins);

            scmd.CommandText = "SET IDENTITY_INSERT series OFF";
            scmd.ExecuteNonQuery();

            //
            // Calendar Series Link
            //
            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress(500 / 6, "Loading Series links");
            ins.CommandText = "INSERT INTO [calendar_series_join] ([sid], [rid]) " +
                "VALUES (@sid, @rid)";
            myadp = new MySqlDataAdapter("SELECT * FROM calendar_series_join", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            scmd.CommandText = "DELETE FROM calendar_series_join";
            scmd.ExecuteNonQuery();

            CopyData(mtable, ins);

            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            strn.Commit();

            scon.Close();
            mcon.Close();

            Db.Compact();
            p.ReportProgress(100, "All done");
        }

        private static void CopyData(DataTable rset, SqlCeCommand ins)
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
