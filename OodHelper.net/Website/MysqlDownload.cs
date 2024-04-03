using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using MySqlConnector;

namespace OodHelper.Website
{
    internal class MySqlDownload
    {
        protected MySqlConnection Mcon;
        protected MySqlTransaction Mtrn;
        protected SqlConnection Scon;
        protected SqlTransaction Strn;

        public MySqlDownload()
        {
            var download = new BackgroundWorker();
            download.DoWork += Process;
            var w = new Working(Application.Current.MainWindow, download);
            download.RunWorkerCompleted += download_RunWorkerCompleted;
            download.RunWorkerAsync();
            w.ShowDialog();
        }

        protected virtual void download_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MessageBox.Show("Download Cancelled", "Cancel", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            else if (e.Result is bool && !(bool)e.Result)
                MessageBox.Show("Download Failed", "Failed", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            else
                MessageBox.Show("Download Complete", "Finished", MessageBoxButton.OK,
                    MessageBoxImage.Information);
        }

        protected void CancelDownload(DoWorkEventArgs e)
        {
            e.Cancel = true;
            try
            {
                Strn.Rollback();
                Strn.Dispose();
                Scon.Close();
                Scon.Dispose();
                Mcon.Close();
                Mcon.Dispose();
            }
            catch
            {
            }
        }

        protected virtual void DoTheWork(object sender, DoWorkEventArgs e)
        {
        }

        private void Process(object sender, DoWorkEventArgs e)
        {
            e.Result = false;
            try
            {
                var p = sender as BackgroundWorker;
                if (p == null)
                    return;

                Scon = new SqlConnection {ConnectionString = Db.DatabaseConstr};
                Scon.Open();
                Strn = Scon.BeginTransaction();

                p.ReportProgress(0, "Connecting to website");

                string mysql = Settings.Mysql;
                var mcsb = new MySqlConnectionStringBuilder(mysql);
                mysql = mcsb.ConnectionString;
                Mcon = new MySqlConnection(mysql);
                Mcon.Open();
                Mtrn = Mcon.BeginTransaction();

                if (p.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }

                DoTheWork(sender, e);

                if (!e.Cancel)
                {
                    Strn.Commit();
                }
                Strn.Dispose();
                Scon.Close();
                Scon.Dispose();

                Db.ReseedDatabase();

                Mcon.Close();
                Mcon.Dispose();

                p.ReportProgress(100, "All done");
                e.Result = true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex);
            }
        }

        protected void CopyData(DataTable rset, SqlCommand ins)
        {
            foreach (DataRow rrow in rset.Rows)
            {
                foreach (DataColumn rc in rset.Columns)
                {
                    ins.Parameters.AddWithValue(rc.ColumnName, rrow[rc.ColumnName]);
                }
                ins.ExecuteNonQuery();
                ins.Parameters.Clear();
            }
        }
    }
}