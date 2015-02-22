using System;
using System.Collections;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace OodHelper
{
    /// <summary>
    ///     Interaction logic for PNImport.xaml
    /// </summary>
    public partial class PnImport
    {
        public PnImport()
        {
            InitializeComponent();
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                FileName = fileName.Text,
                DefaultExt = "*.csv",
                Filter = "CSV files (*.csv)|*.csv"
            };
            var result = dlg.ShowDialog();

            if (result.HasValue && result.Value)
            {
                fileName.Text = dlg.FileName;
                var con = new OdbcConnection("Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq=" +
                                             Path.GetDirectoryName(fileName.Text) +
                                             ";Extended Properties=\"Text;HDR=No;FMT=Delimited\"");
                var da = new OdbcDataAdapter("SELECT * FROM [" + Path.GetFileName(fileName.Text) + "]", con);
                var pi = new DataTable();
                da.Fill(pi);
                con.Close();
                con.Dispose();
                da.Dispose();

                pn.ItemsSource = pi.DefaultView;
            }
        }

        private void import_Click(object sender, RoutedEventArgs e)
        {
            DataTable pi = ((DataView) pn.ItemsSource).Table;

            var del = new Db("DELETE FROM portsmouth_numbers");
            del.ExecuteNonQuery(null);

            var db = new Db(@"INSERT INTO [portsmouth_numbers] 
([id], [class_name], [no_of_crew], [rig], [spinnaker], [engine], [keel], [number], [status], [notes])
VALUES (@id, @class_name, @no_of_crew, @rig, @spinnaker, @engine, @keel, @number, @status, @notes)");

            var expr = new Hashtable();
            foreach (DataRow impr in pi.Rows)
            {
                expr["id"] = Guid.NewGuid();
                expr["class_name"] = impr["ClassName"];
                int tmp;
                if (Int32.TryParse(impr["NoOfCrew"].ToString(), out tmp))
                    expr["no_of_crew"] = tmp;
                else
                    expr["no_of_crew"] = DBNull.Value;
                expr["rig"] = impr["Rig"];
                expr["spinnaker"] = impr["Spinnaker"];
                expr["engine"] = impr["Engine"];
                expr["keel"] = impr["Keel"];
                if (Int32.TryParse(impr["Number"].ToString(), out tmp))
                    expr["number"] = tmp;
                else
                    expr["number"] = DBNull.Value;
                expr["status"] = impr["Status"];
                expr["notes"] = impr["Notes"];
                db.ExecuteNonQuery(expr);
            }
        }
    }
}