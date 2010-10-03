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
    class UploadTide : MySqlUpload
    {
        public UploadTide() : base()
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

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(0, "Loading tide data from file");

            ReadAutoTideData ltd =
                new ReadAutoTideData(@"C:\Documents and Settings\david\My Documents\Peyc Data\at-rosyth-2011.txt");

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(50, "Uploading Tide Data");

            MySqlCommand mcom = new MySqlCommand();
            mcom.Connection = mcon;
            StringBuilder msql = new StringBuilder();

            mcom.CommandText = "DELETE FROM `tidedata` WHERE date >= @date";
            mcom.Parameters.AddWithValue("date", ltd.Data.Rows[0]["date"]);
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `tidedata` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append("INSERT INTO `tidedata` (`date`,`height`,`current`) VALUES ");

            BuildInsertData(ltd.Data, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `tidedata` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }
        }
    }
}
