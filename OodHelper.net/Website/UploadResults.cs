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
    class UploadResults : MySqlUpload
    {
        public UploadResults() : base()
        {
        }

        protected override void upload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                System.Windows.MessageBox.Show("Results Upload Cancelled", "Cancel", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            else
                System.Windows.MessageBox.Show("Results Upload Complete", "Finished", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
        }

        protected override void DoTheWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker w = sender as BackgroundWorker;

            const int Steps = 10;

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(0, "Uploading boats");

            MySqlCommand mcom = new MySqlCommand("DELETE FROM boats");
            mcom.Connection = mcon;
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `boats` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            StringBuilder msql = new StringBuilder("INSERT INTO `boats` (`boatname`,`boatclass`,`sailno`,`dngy`,`h`,`bid`,");
            msql.Append("`distance`,`crewname`,`ohp`,`ohstat`,`chp`,`rhp`,`ihp`,`csf`,`eng`,`kl`,`deviations`,`subscriptn`,`p`,`s`,");
            msql.Append("`boatmemo`,`id`,`beaten`,`berth`,`hired`,`uid`) VALUES ");

            Db c = new Db(@"SELECT boatname, boatclass, sailno, dinghy dngy, hulltype h, bid,
                    distance, crewname, open_handicap ohp, handicap_status ohstat, null chp, rolling_handicap rhp, null ihp, crew_skill_factor csf, engine_propeller eng, keel kl,
                    deviations, subscription subscriptn, p, s, boatmemo, id, beaten, berth, hired, uid
                    FROM boats");
            DataTable d = c.GetData(null);
            c.Dispose();

            BuildInsertData(d, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `boats` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(100 / Steps, "Uploading calendar");

            mcom.CommandText = "DELETE FROM calendar_new";
            mcom.Connection = mcon;
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `calendar_new` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append("INSERT INTO `calendar_new` (`rid`,`start_date`,`time_limit_type`,`time_limit_fixed`,");
            msql.Append("`time_limit_delta`,`extension`,`class`,`event`,`price_code`,`course`,`ood`,`venue`,`racetype`,");
            msql.Append(",`handicapping`,`visitors`,`flag`,`memo`,`is_race`,`raced`,`approved`,`course_choice`,");
            msql.Append("`laps_completed`,`wind_speed`,`wind_direction`,`standard_corrected_time`,`result_calculated`) VALUES ");

            c = new Db(@"SELECT rid, start_date, time_limit_type, time_limit_fixed,
                        time_limit_delta, extension, class, event, price_code, course, ood, venue, racetype,
                        handicapping, visitors, flag, memo, is_race, raced, approved, course_choice,
                        laps_completed, wind_speed, wind_direction, standard_corrected_time, result_calculated
                        FROM calendar");
            d = c.GetData(null);
            c.Dispose();

            BuildInsertData(d, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `calendar_new` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(200 / Steps, "Uploading people");

            mcom.CommandText = "DELETE FROM `people`";
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `people` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append("INSERT INTO `people` (`firstname`,`surname`,`address1`,`address2`,`address3`,`address4`,`postcode`,");
            msql.Append("`hometel`,`worktel`,`mobile`,`email`,`club`,`member`,`cp`,`s`,`id`,`manmemo`,`sid`,`novice`,`uid`,");
            msql.Append("`papernewsletter`,`handbookexclude`) VALUES ");

            c = new Db(@"SELECT firstname,surname,address1,address2,address3,address4,postcode,hometel,worktel,mobile,email,
                        club,member,cp,s,id,manmemo,main_id sid,novice,uid,papernewsletter,handbookexclude
                        FROM people");
            d = c.GetData(null);

            BuildInsertData(d, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `people` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(300 / Steps, "Uploading boat crew");

            mcom.CommandText = "DELETE FROM `boat_crew`";
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `boat_crew` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append("INSERT INTO `boat_crew` (`id`,`bid`) VALUES ");

            c = new Db(@"SELECT id, bid FROM boat_crew");
            d = c.GetData(null);

            BuildInsertData(d, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `boat_crew` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(400 / Steps, "Uploading races");

            mcom.CommandText = "DELETE FROM `races_new`";
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `races_new` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append("INSERT INTO `races_new` (rid,bid,start_date,finish_code,finish_date,finish_date_2,last_edit,laps,place,points,override_points,");
            msql.Append("elapsed,corrected,standard_corrected,handicap_status,open_handicap,rolling_handicap,achieved_handicap,");
            msql.Append("new_rolling_handicap,performance_index,a,c) VALUES ");

            c = new Db(@"SELECT rid,bid,start_date,finish_code,finish_date,finish_date_2,last_edit,laps,place,points,override_points,
                        elapsed,corrected,standard_corrected,handicap_status,open_handicap,rolling_handicap,achieved_handicap,
                        new_rolling_handicap,performance_index,a,c
                        FROM races 
                        WHERE rid IN (SELECT rid FROM calendar WHERE raced = 1)
                        AND (finish_date IS NOT NULL OR finish_code IS NOT NULL)");
            d = c.GetData(null);

            if (d.Rows.Count > 0)
            {
                BuildInsertData(d, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();
            }

            mcom.CommandText = "ALTER TABLE `races_new` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(500 / Steps, "Uploading series");

            mcom.CommandText = "DELETE FROM `series`";
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `series` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append("INSERT INTO `series` (sid,sname,discards) VALUES ");

            c = new Db(@"SELECT sid,sname,discards FROM series");
            d = c.GetData(null);

            BuildInsertData(d, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `series` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(600 / Steps, "Uploading calendar series join");

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

            //
            // Series results
            //
            w.ReportProgress(700 / Steps, "Uploading series results");

            mcom.CommandText = "DELETE FROM `series_results`";
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `series_results` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append("INSERT INTO `series_results` (sid,bid,division,entered,gross,nett,place) VALUES ");

            c = new Db(@"SELECT sid,bid,division,entered,gross,nett,place FROM series_results");
            d = c.GetData(null);

            BuildInsertData(d, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `series_results` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            //
            // select rules
            //

            w.ReportProgress(800 / Steps, "Uploading select rules");

            mcom.CommandText = "DELETE FROM `select_rules`";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append(@"INSERT INTO `select_rules` (`id`,`name`,`parent`,`application`,`field`,`condition`,`string_value`,`number_bound1`,`number_bound2`) VALUES ");

            c = new Db(@"SELECT [id], [name], [parent], [application], [field], [condition], [string_value]
, [number_bound1], [number_bound2] FROM select_rules");
            d = c.GetData(null);

            BuildInsertData(d, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(900 / Steps, "Uploading portsmouth numbers");

            mcom = new MySqlCommand("DELETE FROM `portsmouth_numbers`");
            mcom.Connection = mcon;
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `portsmouth_numbers` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append(@"INSERT INTO `portsmouth_numbers` 
(`id`, `class_name`, `no_of_crew`, `rig`, `spinnaker`, `engine`, `keel`, `number`, `status`, `notes`)
VALUES ");

            c = new Db(@"SELECT id, class_name, no_of_crew, rig, spinnaker, engine, keel, number, status, notes
                    FROM portsmouth_numbers");
            d = c.GetData(null);
            c.Dispose();

            BuildInsertData(d, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `portsmouth_numbers` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            //
            // Recreate update time from itself. This will truncate the time to the second.
            //
            DateTime updateTime = DateTime.Now;
            updateTime = new DateTime(updateTime.Year, updateTime.Month, updateTime.Day, 
                updateTime.Hour, updateTime.Minute, updateTime.Second);

            mcom.CommandText = "INSERT INTO updates (upload, dummy) VALUES (@dt, 2)";
            mcom.Parameters.AddWithValue("dt", updateTime);
            mcom.ExecuteNonQuery();

            c = new Db("INSERT INTO updates (upload, dummy) VALUES (@dt, 2)");
            Hashtable p = new Hashtable();
            p["dt"] = updateTime;
            c.ExecuteNonQuery(p);

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }
        }
    }
}
