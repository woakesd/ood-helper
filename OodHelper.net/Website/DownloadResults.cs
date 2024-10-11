using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using MySqlConnector;

namespace OodHelper.Website
{
    internal class DownloadResults : MySqlDownload
    {
        protected override void DoTheWork(object sender, DoWorkEventArgs e)
        {
            const int steps = 10;
            const double factor = 100 / steps;
            var p = sender as BackgroundWorker;
            if (p == null)
                return;
            var scmd = Scon!.CreateCommand();
            scmd.Transaction = Strn;
            var ins = Scon.CreateCommand();
            ins.Transaction = Strn;
            int progress = 0;
            //
            // Events
            //
            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress((int) ((progress++)*factor), "Loading Calendar");
            ins.CommandText =
                "INSERT INTO [calendar] ([rid], [start_date], [class], [event], [price_code], [course], [ood], " +
                "[venue], [racetype], [handicapping], [visitors], [flag], [time_limit_type], [time_limit_fixed], " +
                "[time_limit_delta], [extension], [memo], [is_race], [raced], [approved], [course_choice], [laps_completed], " +
                "[wind_speed], [wind_direction], [standard_corrected_time], [result_calculated]) " +
                "VALUES (@rid, @start_date, @class, @event, @price_code, @course, @ood, @venue, @racetype, " +
                "@handicapping, @visitors, @flag, @time_limit_type, @time_limit_fixed, @time_limit_delta, @extension, @memo, " +
                "@is_race, @raced, @approved, @course_choice, @laps_completed, @wind_speed, @wind_direction, " +
                "@standard_corrected_time, @result_calculated)";

            var myadp = new MySqlDataAdapter(new MySqlCommand(
                "SELECT `rid`, `start_date`, `class`, `event`, `price_code`, `course`, `ood`, `venue`, " +
                "`racetype`, `handicapping`, `visitors`, `flag`, `time_limit_type`, `time_limit_fixed`, " +
                "`time_limit_delta`, `extension`, `memo`, `is_race`, `raced`, `approved`, `course_choice`, `laps_completed`, " +
                "`wind_speed`, `wind_direction`, `standard_corrected_time`, `result_calculated` FROM calendar_new",
                Mcon, Mtrn));
            var mtable = new DataTable();
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

            p.ReportProgress((int) ((progress++)*factor), "Loading People");
            ins.CommandText =
                "INSERT INTO [PEOPLE] ([id], [main_id], [firstname], [surname], [address1], [address2], [address3], [address4], [postcode], [hometel], [worktel], [mobile], [email], [club], [member], [cp], [s], [manmemo], [novice], [uid], [papernewsletter], [handbookexclude]) " +
                "VALUES (@id, @sid, @firstname, @surname, @address1, @address2, @address3, @address4, @postcode, @hometel, @worktel, @mobile, @email, @club, @member, @cp, @s, @manmemo, @novice,@uid,@papernewsletter,@handbookexclude)";

            myadp = new MySqlDataAdapter(new MySqlCommand("SELECT * FROM people", Mcon, Mtrn));
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
            // Boats
            //
            p.ReportProgress((int) ((progress++)*factor), "Loading Boats");
            
            myadp = new MySqlDataAdapter(new MySqlCommand("SELECT * FROM boats", Mcon, Mtrn));
            mtable = new DataTable();
            myadp.Fill(mtable);
            scmd.CommandText = "DELETE FROM boats";
            scmd.ExecuteNonQuery();
            ins.CommandText = "INSERT INTO [boats] " + "([bid], [id], [boatname], [boatclass], [sailno], [dinghy], [hulltype], [distance], [crewname], [open_handicap], [handicap_status], [rolling_handicap], [crew_skill_factor], [engine_propeller], [keel], [deviations], [subscription], [berth], [boatmemo], [hired], [p], [s], [beaten],[uid]) " + "VALUES " + "(@bid, @id, @boatname, @boatclass, @sailno, @dngy, @h, @distance, @crewname, @ohp, @ohstat, @rhp, @csf, @eng, @kl, @deviations, @subscriptn, @berth, @boatmemo, @hired, @p, @s, @beaten, @uid)";
            scmd.CommandText = "SET IDENTITY_INSERT boats ON";
            scmd.ExecuteNonQuery();
            CopyData(mtable, ins);
            scmd.CommandText = "SET IDENTITY_INSERT boats OFF";
            scmd.ExecuteNonQuery();
            //
            // Boat Crew
            //
            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress((int) ((progress++)*factor), "Loading Boat Crew");
            ins.CommandText = "INSERT INTO [boat_crew] ([id], [bid]) " +
                              "VALUES (@id, @bid)";

            myadp = new MySqlDataAdapter(new MySqlCommand("SELECT * FROM boat_crew", Mcon, Mtrn));
            mtable = new DataTable();
            myadp.Fill(mtable);
            scmd.CommandText = "DELETE FROM boat_crew";
            scmd.ExecuteNonQuery();
            CopyData(mtable, ins);
            //
            // Races
            //
            p.ReportProgress((int) ((progress++)*factor), "Loading Races");
            ins.CommandText =
                "INSERT INTO [races] ([rid], [bid], [start_date], [finish_date], [interim_date], [restricted_sail], [last_edit], [finish_code], [laps], [elapsed], [corrected], [standard_corrected], [handicap_status], [open_handicap], [rolling_handicap], [achieved_handicap], [new_rolling_handicap], [place], [points], [override_points], [performance_index], [a], [c]) " +
                "VALUES (@rid, @bid, @start_date, @finish_date, @interim_date, @restricted_sail, @last_edit, @finish_code, @laps, @elapsed, @corrected, @standard_corrected, @handicap_status, @open_handicap, @rolling_handicap, @achieved_handicap, @new_rolling_handicap, @place, @points, @override_points, @performance_index, @a, @c)";

            myadp = new MySqlDataAdapter(
                new MySqlCommand("SELECT `rid`, `bid`, `start_date`, `finish_date`, `interim_date`, `restricted_sail`, `last_edit`, `finish_code`, `laps`, `elapsed`, `corrected`, `standard_corrected`, `handicap_status`, `open_handicap`, `rolling_handicap`, `achieved_handicap`, `new_rolling_handicap`, `place`, `points`, `override_points`, `performance_index`, `a`, `c` FROM races_new", Mcon, Mtrn));
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

            p.ReportProgress((int) ((progress++)*factor), "Loading Series");
            ins.CommandText = "INSERT INTO [series] ([sid], [sname], [discards]) " +
                              "VALUES (@sid, @sname, @discards)";
            myadp = new MySqlDataAdapter(new MySqlCommand("SELECT sid, sname, discards FROM series", Mcon, Mtrn));
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

            p.ReportProgress((int) ((progress++)*factor), "Loading Series links");
            ins.CommandText = "INSERT INTO [calendar_series_join] ([sid], [rid]) " +
                              "VALUES (@sid, @rid)";
            myadp = new MySqlDataAdapter(new MySqlCommand("SELECT * FROM calendar_series_join", Mcon, Mtrn));
            mtable = new DataTable();
            myadp.Fill(mtable);
            scmd.CommandText = "DELETE FROM calendar_series_join";
            scmd.ExecuteNonQuery();
            CopyData(mtable, ins);
            //
            // Series Results
            //
            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            p.ReportProgress((int) ((progress++)*factor), "Loading Series Results");
            ins.CommandText =
                "INSERT INTO [series_results] ([sid], [bid], [division], [entered], [gross], [nett], [place]) " +
                "VALUES (@sid, @bid, @division, @entered, @gross, @nett, @place)";
            myadp = new MySqlDataAdapter(new MySqlCommand("SELECT * FROM series_results", Mcon, Mtrn));
            mtable = new DataTable();
            myadp.Fill(mtable);
            scmd.CommandText = "DELETE FROM series_results";
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

            p.ReportProgress((int) ((progress++)*factor), "Loading Select Rules");
            ins.CommandText =
                "INSERT INTO [select_rules] ([id], [name], [parent], [application], [field], [condition], [string_value], [number_bound1], [number_bound2]) " +
                "VALUES (@id, @name, @parent, @application, @field, @condition, @string_value, @number_bound1, @number_bound2)";
            myadp = new MySqlDataAdapter(new MySqlCommand("SELECT * FROM select_rules", Mcon, Mtrn));
            mtable = new DataTable();
            myadp.Fill(mtable);
            scmd.CommandText = "DELETE FROM select_rules";
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

            p.ReportProgress((int)((progress) * factor), "Loading Portsmouth numbers");
            ins.CommandText = @"INSERT INTO [portsmouth_numbers] 
([id], [class_name], [no_of_crew], [rig], [spinnaker], [engine], [keel], [number], [status], [notes])
VALUES (@id, @class_name, @no_of_crew, @rig, @spinnaker, @engine, @keel, @number, @status, @notes)";
            myadp = new MySqlDataAdapter(new MySqlCommand("SELECT * FROM portsmouth_numbers", Mcon, Mtrn));
            mtable = new DataTable();
            myadp.Fill(mtable);
            scmd.CommandText = "DELETE FROM portsmouth_numbers";
            scmd.ExecuteNonQuery();
            CopyData(mtable, ins);
            //
            // Find max update row from website db and insert into local db.
            //
            myadp = new MySqlDataAdapter(new MySqlCommand("SELECT MAX(upload) FROM updates", Mcon, Mtrn));
            mtable = new DataTable();
            myadp.Fill(mtable);
            if (mtable.Rows.Count > 0)
            {
                scmd.CommandText = "INSERT INTO updates (upload, dummy) VALUES (@dt, 2)";
                scmd.Parameters.Clear();
                scmd.Parameters.AddWithValue("dt", mtable.Rows[0][0]);
                scmd.ExecuteNonQuery();
            }

            if (p.CancellationPending)
            {
                CancelDownload(e);
            }
        }
    }
}