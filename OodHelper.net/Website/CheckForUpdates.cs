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
            else
                System.Windows.MessageBox.Show("Check For Updates Complete", "Finished", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
        }

        protected override void DoTheWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker p = sender as BackgroundWorker;

            p.ReportProgress(0, "Checking for updates");

            //
            // Boats
            //
            MySqlDataAdapter myadp = new MySqlDataAdapter("SELECT MAX(date)", mcon);
            DataTable mtable = new DataTable();
            myadp.Fill(mtable);

            if (p.CancellationPending)
            {
                CancelDownload(e);
                return;
            }
        }
    }
}
