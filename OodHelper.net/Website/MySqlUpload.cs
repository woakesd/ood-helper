using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data.SqlServerCe;
using System.ComponentModel;

namespace OodHelper.Website
{
    class MySqlUpload
    {
        private Working p;
        private BackgroundWorker upload;

        public MySqlUpload() : this(false)
        {
            Run();
        }

        public MySqlUpload(bool run)
        {
            upload = new BackgroundWorker();
            upload.DoWork += new DoWorkEventHandler(Process);
            p = new Working(upload);
            upload.RunWorkerCompleted += new RunWorkerCompletedEventHandler(upload_RunWorkerCompleted);
        }

        public void Run()
        {
            upload.RunWorkerAsync();
            p.ShowDialog();
        }

        protected virtual void upload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                System.Windows.MessageBox.Show("Upload Cancelled", "Cancel", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            else
                System.Windows.MessageBox.Show("Upload Complete", "Finished", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
        }

        protected void CancelDownload(DoWorkEventArgs e)
        {
            e.Cancel = true;
            try
            {
                mtrn.Commit();
                mtrn.Dispose();
                mcon.Close();
                mcon.Dispose();
            }
            catch
            {
            }
            return;
        }

        protected MySqlConnection mcon;
        protected MySqlTransaction mtrn;

        protected virtual void DoTheWork(object sender, DoWorkEventArgs e)
        {
        }

        private void Process(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker w = sender as BackgroundWorker;

                if (w.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }

                w.ReportProgress(0, "Connecting to Website");

                string mysql = (string)DbSettings.GetSetting("mysql");
                MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(mysql);
                mysql = mcsb.ConnectionString;
                mcon = new MySqlConnection(mysql);
                mcon.Open();
                mtrn = mcon.BeginTransaction();

                DoTheWork(sender, e);

                if (!e.Cancel)
                {
                    mtrn.Commit();

                    mcon.Close();
                    mcon.Dispose();

                    w.ReportProgress(100, "All done");
                }
            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show(exp.Message, "Error", System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        protected void BuildInsertData(DataTable d, StringBuilder msql)
        {
            for (int i = 0; i < d.Rows.Count; i++)
            {
                DataRow dr = d.Rows[i];

                msql.Append("(");
                for (int j = 0; j < d.Columns.Count; j++)
                {
                    switch (d.Columns[j].DataType.ToString())
                    {
                        case "System.String":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("'{0}'", dr[j].ToString().Replace("'", "''"));
                            else
                                msql.Append("NULL");
                            break;
                        case "System.Int32":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("{0}", dr[j]);
                            else
                                msql.Append("NULL");
                            break;
                        case "System.Double":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("{0}", dr[j]);
                            else
                                msql.Append("NULL");
                            break;
                        case "System.DateTime":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("'{0:yyyy-MM-dd HH:mm:ss}'", dr[j]);
                            else
                                msql.Append("NULL");
                            break;
                        case "System.Boolean":
                            if (dr[j] == DBNull.Value)
                                msql.Append("NULL");
                            else if ((bool)dr[j])
                                msql.Append("1");
                            else
                                msql.Append("0");
                            break;
                    }
                    if (j < d.Columns.Count - 1) msql.Append(",");
                }
                msql.Append(")");
                if (i < d.Rows.Count - 1) msql.Append(",");
            }
        }
    }
}
