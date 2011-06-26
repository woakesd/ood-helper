using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.SqlServerCe;
using System.ComponentModel;

namespace OodHelper.Website
{
    class CheckForUpdates : MySqlDownload
    {
        public CheckForUpdates()
            : base()
        {
        }

        protected override void download_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                System.Windows.MessageBox.Show("Check For Updates Cancelled", "Cancel", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
        }

        public DateTime? RemoteDate { get; set; }
        public DateTime? LocalDate { get; set; }

        protected override void DoTheWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker p = sender as BackgroundWorker;

            p.ReportProgress(0, "Checking for updates");

            //
            // Find max update row from website db and local db
            //
            MySqlDataAdapter myadp = new MySqlDataAdapter("SELECT MAX(upload) FROM updates", mcon);
            DataTable mtable = new DataTable();
            myadp.Fill(mtable);

            if (mtable.Rows.Count > 0)
            {
                RemoteDate = mtable.Rows[0][0] as DateTime?;
            }

            SqlCeDataAdapter sqadp = new SqlCeDataAdapter("SELECT MAX(upload) FROM updates", scon);
            DataTable stable = new DataTable();
            sqadp.Fill(stable);

            if (mtable.Rows.Count > 0)
            {
                LocalDate = stable.Rows[0][0] as DateTime?;
            }

            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }
        }
    }
}
