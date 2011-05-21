using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.SqlServerCe;
using System.ComponentModel;
using OodHelper;
using OodHelper.LoadTide;

namespace OodHelper.Website
{
    [Svn("$Id$")]
    class UploadSun : MySqlUpload
    {
        private DataTable SunData;

        public UploadSun(DataTable dt) : base(false)
        {
            SunData = dt;
            Run();
        }

        protected override void upload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                System.Windows.MessageBox.Show("Sun Data Upload Cancelled", "Cancel", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            else
                System.Windows.MessageBox.Show("Sun Data Upload Complete", "Finished", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
        }

        protected override void DoTheWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker w = sender as BackgroundWorker;

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(50, "Uploading Sun Data");

            MySqlCommand mcom = new MySqlCommand();
            mcom.Connection = mcon;
            StringBuilder msql = new StringBuilder();

            mcom.CommandText = "DELETE FROM `sun` WHERE date >= @start AND date <= @end";
            mcom.Parameters.AddWithValue("start", SunData.Rows[0]["date"]);
            mcom.Parameters.AddWithValue("end", SunData.Rows[SunData.Rows.Count-1]["date"]);
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `sun` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append("INSERT INTO `sun` (`date`,`sunrise`,`sunset`) VALUES ");

            BuildInsertData(SunData, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `sun` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }
        }
    }
}
