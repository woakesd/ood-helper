using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using MySqlConnector;

namespace OodHelper.Website
{
    internal class CheckForUpdates : MySqlDownload
    {
        public DateTime? RemoteDate { get; set; }

        public DateTime? LocalDate { get; set; }

        protected override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MessageBox.Show("Check For Updates Cancelled", "Cancel", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (e.Result is bool && !(bool)e.Result)
                MessageBox.Show("Check For Updates Failed", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void DoTheWork(object sender, DoWorkEventArgs e)
        {
            var p = sender as BackgroundWorker;
            if (p == null)
                return;
            p.ReportProgress(0, "Checking for updates");
            //
            // Find max update row from website db and local db
            //
            var cmd = new MySqlCommand("SELECT MAX(upload) FROM updates", Mcon, Mtrn);
            RemoteDate = cmd.ExecuteScalar() as DateTime?;

            var localCmd = new SqlCommand("SELECT MAX(upload) FROM updates", Scon, Strn);
            LocalDate = cmd.ExecuteScalar() as DateTime?;

            if (p.CancellationPending)
            {
                CancelDownload(e);
            }

            e.Result = true;
        }
    }
}