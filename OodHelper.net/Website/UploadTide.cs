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
        DataTable Tide { get; set; }
        public UploadTide(DataTable tide) : base(false)
        {
            Tide = tide;
            Run();
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

            w.ReportProgress(0, "Uploading Tide Data");

            MySqlCommand mcom = new MySqlCommand();
            mcom.Connection = mcon;
            StringBuilder msql = new StringBuilder();

            mcom.CommandText = "DELETE FROM `tidedata` WHERE date >= @sdate AND date <= @edate";
            mcom.Parameters.AddWithValue("edate", Tide.Rows[0]["date"]);
            mcom.Parameters.AddWithValue("sdate", Tide.Rows[Tide.Rows.Count-1]["date"]);
            int n = mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `tidedata` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            int i = 0;
            while (i < Tide.Rows.Count)
            {
                DataTable sub = Tide.Clone();
                for (int j = 0; j + i < Tide.Rows.Count && j < 1000; j++)
                {
                    sub.ImportRow(Tide.Rows[i + j]);
                }

                msql.Clear();

                msql.Append("INSERT INTO `tidedata` (`date`,`height`,`current`) VALUES ");

                BuildInsertData(sub, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();

                i += 1000;

                w.ReportProgress((int) (((double)i) / Tide.Rows.Count * 100), "Uploading Tide Data");

                if (w.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }
            }

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
