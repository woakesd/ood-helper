using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.SqlServerCe;
using System.ComponentModel;

namespace OodHelper.Website
{
    [Svn("$Id$")]
    class DownloadResults : MySqlDownload
    {
        public DownloadResults() : base()
        {
        }

        protected override void download_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                System.Windows.MessageBox.Show("Results Download Cancelled", "Cancel", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            else
                System.Windows.MessageBox.Show("Results Download Complete", "Finished", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
        }

        protected override void DoTheWork(object sender, DoWorkEventArgs e)
        {
            const int Steps = 8;
            BackgroundWorker p = sender as BackgroundWorker;

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

            p.ReportProgress(100 / Steps, "Loading Calendar");
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

            p.ReportProgress(200 / Steps, "Loading People");
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
            // Boat Crew
            //
            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress(300 / Steps, "Loading Boat Crew");
            ins.CommandText = "INSERT INTO [boat_crew] ([id], [bid]) " +
                "VALUES (@id, @bid)";

            myadp = new MySqlDataAdapter("SELECT * FROM boat_crew", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            scmd.CommandText = "DELETE FROM boat_crew";
            scmd.ExecuteNonQuery();

            CopyData(mtable, ins);

            //
            // Races
            //
            p.ReportProgress(400 / Steps, "Loading Races");
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

            p.ReportProgress(500 / Steps, "Loading Series");
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

            p.ReportProgress(600 / Steps, "Loading Series links");
            ins.CommandText = "INSERT INTO [calendar_series_join] ([sid], [rid]) " +
                "VALUES (@sid, @rid)";
            myadp = new MySqlDataAdapter("SELECT * FROM calendar_series_join", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            scmd.CommandText = "DELETE FROM calendar_series_join";
            scmd.ExecuteNonQuery();

            CopyData(mtable, ins);

            //
            // Select Rules
            //
            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress(700 / Steps, "Loading Select Rules");
            ins.CommandText = "INSERT INTO [select_rules] ([id], [name], [parent], [application], [field], [condition], [string_value], [number_bound1], [number_bound2]) " +
                "VALUES (@id, @name, @parent, @application, @field, @condition, @string_value, @number_bound1, @number_bound2)";
            myadp = new MySqlDataAdapter("SELECT * FROM select_rules", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            scmd.CommandText = "DELETE FROM select_rules";
            scmd.ExecuteNonQuery();

            CopyData(mtable, ins);

            //
            // Find max update row from website db and insert into local db.
            //
            myadp = new MySqlDataAdapter("SELECT MAX(upload) FROM updates", mcon);
            mtable = new DataTable();
            myadp.Fill(mtable);

            if (mtable.Rows.Count > 0)
            {
                scmd.CommandText = "INSERT INTO updates (upload, dummy) VALUES (@dt, 2)";
                scmd.Parameters.Clear();
                scmd.Parameters.Add("dt", mtable.Rows[0][0]);
                scmd.ExecuteNonQuery();
            }

            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }
        }
    }
}
