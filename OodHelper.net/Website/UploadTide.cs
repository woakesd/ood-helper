using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows;
using MySqlConnector;

namespace OodHelper.Website
{
    internal class UploadTide : MySqlUpload
    {
        public UploadTide(DataTable tide)
        {
            Tide = tide;
            Run();
        }

        private DataTable Tide { get; set; }

        protected override void upload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MessageBox.Show("Results Upload Cancelled", "Cancel", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            else
                MessageBox.Show("Results Upload Complete", "Finished", MessageBoxButton.OK,
                    MessageBoxImage.Information);
        }

        protected override void DoTheWork(object sender, DoWorkEventArgs e)
        {
            var w = sender as BackgroundWorker;

            if (w == null) return;

            if (w.CancellationPending)
            {
                CancelDownload(e);
                return;
            }

            w.ReportProgress(0, "Uploading Tide Data");

            var mcom = new MySqlCommand {Connection = Mcon, Transaction = Mtrn };
            var msql = new StringBuilder();

            mcom.CommandText = "DELETE FROM `tidedata` WHERE date >= @sdate AND date <= @edate";
            mcom.Parameters.AddWithValue("sdate", Tide.Rows[0]["date"]);
            mcom.Parameters.AddWithValue("edate", Tide.Rows[Tide.Rows.Count - 1]["date"]);
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `tidedata` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            var i = 0;
            while (i < Tide.Rows.Count)
            {
                var sub = Tide.Clone();
                for (var j = 0; j + i < Tide.Rows.Count && j < 1000; j++)
                {
                    sub.ImportRow(Tide.Rows[i + j]);
                }

                msql.Clear();

                msql.Append("INSERT INTO `tidedata` (`date`,`height`,`current`, `flow`, `tide`) VALUES ");

                BuildInsertData(sub, msql);

                mcom.CommandText = msql.ToString();
                mcom.ExecuteNonQuery();

                i += 1000;

                w.ReportProgress((int) (((double) i)/Tide.Rows.Count*100), "Uploading Tide Data");

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
            }
        }
    }
}