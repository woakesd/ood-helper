using System;
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

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for PNImport.xaml
    /// </summary>
    [Svn("$Id$")]
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
            dlg.Filter = "Comma Seperated Value files (*.csv)|*.csv";
            Nullable<bool> result = dlg.ShowDialog();

            if (result.Value)
            {
                fileName.Text = dlg.FileName;
            }
        }

        private void import_Click(object sender, RoutedEventArgs e)
        {
            OdbcConnection con = new OdbcConnection("Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq=" +
                Path.GetDirectoryName(fileName.Text) + ";Extended Properties=\"Text;HDR=No;FMT=Delimited\"");
            OdbcDataAdapter da = new OdbcDataAdapter("SELECT * FROM [" + Path.GetFileName(fileName.Text) + "]", con);
            DataTable pi = new DataTable();
            da.Fill(pi);
            con.Close();
            con.Dispose();
            da.Dispose();

            HandicapDb hdb = new HandicapDb("DELETE FROM portsmouth_numbers");
            hdb.ExecuteNonQuery(null);

            hdb = new HandicapDb("SELECT * FROM portsmouth_numbers");
            DataTable pt = new DataTable();
            hdb.Fill(pt, null);

            foreach (DataRow impr in pi.Rows)
            {
                DataRow expr = pt.NewRow();
                expr["class_name"] = impr["ClassName"];
                expr["no_of_crew"] = impr["NoOfCrew"];
                expr["rig"] = impr["Rig"];
                expr["spinnaker"] = impr["Spinnaker"];
                expr["engine"] = impr["Engine"];
                expr["keel"] = impr["Keel"];
                expr["number"] = impr["Number"];
                expr["status"] = impr["Status"];
                expr["notes"] = impr["Notes"];
                pt.Rows.Add(expr);
            }
            hdb.Commit(pt);
            pn.ItemsSource = pt.DefaultView;
        }
    }
}
