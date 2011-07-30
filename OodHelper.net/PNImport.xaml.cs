using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for PNImport.xaml
    /// </summary>
    public partial class PNImport : Window
    {
        public PNImport()
        {
            InitializeComponent();
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = fileName.Text;
            dlg.DefaultExt = "*.csv";
            dlg.Filter = "CSV files (*.csv)|*.csv";
            bool? result = dlg.ShowDialog();

            if (result.Value)
            {
                fileName.Text = dlg.FileName;
                OdbcConnection con = new OdbcConnection("Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq=" +
    Path.GetDirectoryName(fileName.Text) + ";Extended Properties=\"Text;HDR=No;FMT=Delimited\"");
                OdbcDataAdapter da = new OdbcDataAdapter("SELECT * FROM [" + Path.GetFileName(fileName.Text) + "]", con);
                DataTable pi = new DataTable();
                da.Fill(pi);
                con.Close();
                con.Dispose();
                da.Dispose();

                pn.ItemsSource = pi.DefaultView;
            }
        }

        private void import_Click(object sender, RoutedEventArgs e)
        {
            DataTable pi = ((DataView)pn.ItemsSource).Table;

            Db del = new Db("DELETE FROM portsmouth_numbers");
            del.ExecuteNonQuery(null);

            Db db = new Db(@"INSERT INTO [portsmouth_numbers] 
([id], [class_name], [no_of_crew], [rig], [spinnaker], [engine], [keel], [number], [status], [notes])
VALUES (@id, @class_name, @no_of_crew, @rig, @spinnaker, @engine, @keel, @number, @status, @notes)");

            Hashtable expr = new Hashtable();
            foreach (DataRow impr in pi.Rows)
            {
                expr["id"] = Guid.NewGuid();
                expr["class_name"] = impr["ClassName"];
                int tmp;
                if (Int32.TryParse(impr["NoOfCrew"].ToString(), out tmp))
                    expr["no_of_crew"] =  tmp;
                else
                    expr["no_of_crew"] =  DBNull.Value;
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
                int rowdone = db.ExecuteNonQuery(expr);
            }
        }
    }
}
