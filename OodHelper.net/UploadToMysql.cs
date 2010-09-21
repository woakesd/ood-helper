using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data.SqlServerCe;
using System.ComponentModel;

namespace OodHelper.net
{
    class UploadToMysql
    {
        public UploadToMysql()
        {
            BackgroundWorker upload = new BackgroundWorker();
            upload.DoWork += new DoWorkEventHandler(Process);
            Working p = new Working(System.Windows.Application.Current.MainWindow, upload);
            upload.RunWorkerCompleted += new RunWorkerCompletedEventHandler(upload_RunWorkerCompleted);
            upload.RunWorkerAsync();
            p.ShowDialog();
        }

        public void upload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                System.Windows.MessageBox.Show("Upload Cancelled", "Cancel", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            else
                System.Windows.MessageBox.Show("Upload Complete", "Finished", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
        }

        private void CancelDownload(DoWorkEventArgs e)
        {
            e.Cancel = true;
            try
            {
                mtrn.Commit();
                mtrn.Dispose();
                mcon.Close();
                mcon.Dispose();
            }
            catch
            {
            }
            return;
        }

        MySqlConnection mcon;
        MySqlTransaction mtrn;

        public void Process(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker w = sender as BackgroundWorker;

                if (w.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }

                w.ReportProgress(0, "Uploading boats");
                string mysql = (string)DbSettings.GetSetting("mysql");
                MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(mysql);
                mysql = mcsb.ConnectionString;
                mcon = new MySqlConnection(mysql);
                mcon.Open();
                mtrn = mcon.BeginTransaction();

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

                if (w.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }

                w.ReportProgress(100 / 6, "Uploading calendar");

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

                if (w.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }

                w.ReportProgress(200 / 6, "Uploading people");

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

                if (w.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }

                w.ReportProgress(300 / 6, "Uploading races");

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
                        FROM races 
                        WHERE rid IN (SELECT rid FROM calendar WHERE raced = 1)
                        AND (finish_date IS NOT NULL OR finish_code IS NOT NULL)");
                d = c.GetData(null);

                BuildInsertData(d, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();

                mcom.CommandText = "ALTER TABLE `races_new` ENABLE KEYS";
                mcom.ExecuteNonQuery();

                if (w.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }

                w.ReportProgress(400 / 6, "Uploading series");

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

                w.ReportProgress(500 / 6, "Uploading calendar series join");

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

                DateTime updateTime = DateTime.Now;

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

                mtrn.Commit();

                w.ReportProgress(100, "All done");
            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show(exp.Message, "Error", System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
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
    }
}
