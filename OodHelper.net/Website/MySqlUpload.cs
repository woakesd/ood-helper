using System;
using System.Data;
using System.Text;
using System.Windows;
using MySqlConnector;
using System.ComponentModel;

namespace OodHelper.Website
{
    class MySqlUpload
    {
        private readonly Working _p;
        private readonly BackgroundWorker _upload;

        public MySqlUpload()
        {
            _upload = new BackgroundWorker();
            _upload.DoWork += Process;
            _p = new Working(Application.Current.MainWindow, _upload);
            _upload.RunWorkerCompleted += upload_RunWorkerCompleted;
            //_upload.RunWorkerAsync();
            //_p.ShowDialog();
        }

        public void Run()
        {
            _upload.RunWorkerAsync();
            _p.ShowDialog();
        }

        protected virtual void upload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MessageBox.Show("Upload Cancelled", "Cancel", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            else if (e.Result is bool && !(bool)e.Result)
                MessageBox.Show("Upload Failed", "Failed", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            else
                MessageBox.Show("Upload Complete", "Finished", MessageBoxButton.OK,
                    MessageBoxImage.Information);
        }

        protected void CancelDownload(DoWorkEventArgs e)
        {
            e.Cancel = true;
            try
            {
                Mtrn.Rollback();
                Mtrn.Dispose();
                Mcon.Close();
                Mcon.Dispose();
            }
            catch
            {
            }
        }

        protected MySqlConnection Mcon;
        protected MySqlTransaction Mtrn;

        protected virtual void DoTheWork(object sender, DoWorkEventArgs e)
        {
        }

        private void Process(object sender, DoWorkEventArgs e)
        {
            e.Result = false;
            try
            {
                var w = sender as BackgroundWorker;
                if (w == null)
                    return;

                if (w.CancellationPending)
                {
                    CancelDownload(e);
                    return;
                }

                w.ReportProgress(0, "Connecting to Website");

                string mysql = Settings.Mysql;
                var mcsb = new MySqlConnectionStringBuilder(mysql);
                mysql = mcsb.ConnectionString;
                Mcon = new MySqlConnection(mysql);
                Mcon.Open();
                Mtrn = Mcon.BeginTransaction();

                DoTheWork(sender, e);

                if (!e.Cancel)
                {
                    Mtrn.Commit();

                    Mcon.Close();
                    Mcon.Dispose();

                    w.ReportProgress(100, "All done");

                    PushResultNotification.ResultPublished();
                }
                e.Result = true;
            }
            catch (Exception exp)
            {
                ErrorLogger.LogException(exp);
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
                    string colType = d.Columns[j].DataType.ToString();
                    switch (colType)
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
                            if (dr[j] != DBNull.Value && !Double.IsNaN((double)dr[j]))
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
                        case "System.Decimal":
                            if (dr[j] == DBNull.Value)
                                msql.Append("NULL");
                            else
                                msql.AppendFormat("{0}", dr[j]);
                            break;
                        case "System.Guid":
                            if (dr[j] == DBNull.Value)
                                msql.Append("NULL");
                            else
                                msql.AppendFormat("'{{{0}}}'", dr[j]);
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
