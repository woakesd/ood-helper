using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;
using Microsoft.Data.SqlClient;

namespace OodHelper.Website
{
    internal class CheckForUpdates : MySqlDownload
    {
        public DateTime? RemoteDate { get; set; }

        public DateTime? LocalDate { get; set; }

        protected override void download_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            var myadp = new MySqlDataAdapter("SELECT MAX(upload) FROM updates", Mcon);
            var mtable = new DataTable();
            myadp.Fill(mtable);
            myadp.Dispose();
            if (mtable.Rows.Count > 0)
            {
                RemoteDate = mtable.Rows[0][0] as DateTime? ;
            }

            var sqadp = new SqlDataAdapter(new SqlCommand("SELECT MAX(upload) FROM updates", Scon, Strn));
            var stable = new DataTable();
            sqadp.Fill(stable);
            sqadp.Dispose();
            if (mtable.Rows.Count > 0)
            {
                LocalDate = stable.Rows[0][0] as DateTime? ;
            }

            if (p.CancellationPending)
            {
                CancelDownload(e);
            }

            e.Result = true;
        }
    }
}