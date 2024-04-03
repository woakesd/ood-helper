using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows;
using MySqlConnector;

namespace OodHelper.Website
{
    internal class UploadSun : MySqlUpload
    {
        private readonly DataTable _sunData;

        public UploadSun(DataTable dt)
        {
            _sunData = dt;
            Run();
        }

        protected override void upload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MessageBox.Show("Sun Data Upload Cancelled", "Cancel", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            else
                MessageBox.Show("Sun Data Upload Complete", "Finished", MessageBoxButton.OK,
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

            w.ReportProgress(50, "Uploading Sun Data");

            var mcom = new MySqlCommand {Connection = Mcon, Transaction = Mtrn};
            var msql = new StringBuilder();

            mcom.CommandText = "DELETE FROM `sun` WHERE date >= @start AND date <= @end";
            mcom.Parameters.AddWithValue("start", _sunData.Rows[0]["date"]);
            mcom.Parameters.AddWithValue("end", _sunData.Rows[_sunData.Rows.Count - 1]["date"]);
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `sun` DISABLE KEYS";
            mcom.ExecuteNonQuery();

            msql.Clear();
            msql.Append("INSERT INTO `sun` (`date`,`sunrise`,`sunset`) VALUES ");

            BuildInsertData(_sunData, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mcom.CommandText = "ALTER TABLE `sun` ENABLE KEYS";
            mcom.ExecuteNonQuery();

            if (w.CancellationPending)
            {
                CancelDownload(e);
            }
        }
    }
}