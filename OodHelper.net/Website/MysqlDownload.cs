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
    class MySqlDownload
    {
        public MySqlDownload()
        {
            BackgroundWorker download = new BackgroundWorker();
            download.DoWork += new DoWorkEventHandler(Process);
            Working w = new Working(download);
            download.RunWorkerCompleted += new RunWorkerCompletedEventHandler(download_RunWorkerCompleted);
            download.RunWorkerAsync();
            w.ShowDialog();
        }
        
        protected SqlCeConnection scon;
        protected SqlCeTransaction strn;
        protected MySqlConnection mcon;

        protected virtual void download_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                System.Windows.MessageBox.Show("Download Cancelled", "Cancel", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            else
                System.Windows.MessageBox.Show("Download Complete", "Finished", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
        }

        protected void CancelDownload(DoWorkEventArgs e)
        {
            e.Cancel = true;
            try
            {
                strn.Rollback();
                strn.Dispose();
                scon.Close();
                scon.Dispose();
                mcon.Close();
                mcon.Dispose();
            }
            catch
            {
            }
            return;
        }

        protected virtual void DoTheWork(object sender, DoWorkEventArgs e)
        {
        }

        private void Process(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker p = sender as BackgroundWorker;

                scon = new SqlCeConnection();
                scon.ConnectionString = Properties.Settings.Default.OodHelperConnectionString;
                scon.Open();
                strn = scon.BeginTransaction();

                p.ReportProgress(0, "Connecting to website");

                string mysql = (string)DbSettings.GetSetting("mysql");
                MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(mysql);
                mysql = mcsb.ConnectionString;
                mcon = new MySqlConnection(mysql);
                mcon.Open();

                if (p.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }

                DoTheWork(sender, e);

                if (!e.Cancel)
                {
                    strn.Commit();

                    scon.Close();

                    Db.Compact();
                }
                p.ReportProgress(100, "All done");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex);
                throw;
            }
        }

        protected void CopyData(DataTable rset, SqlCeCommand ins)
        {
            foreach (DataRow rrow in rset.Rows)
            {
                foreach (DataColumn rc in rset.Columns)
                {
                    ins.Parameters.Add(rc.ColumnName, rrow[rc.ColumnName]);
                }
                SqlCeParameter p1 = ins.Parameters[1];
                ins.ExecuteNonQuery();
                ins.Parameters.Clear();
            }
        }
    }
}
