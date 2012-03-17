using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using MySql.Data.MySqlClient;

namespace OodHelper.LoadTide
{
    /// <summary>
    /// Interaction logic for ReadData.xaml
    /// </summary>
    public partial class ReadData : Window
    {
        ReadFormat11 TideInfo { get; set; }

        public ReadData()
        {
            InitializeComponent();
            TideInfo = new ReadFormat11();
            DataContext = TideInfo;
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Format11 files; (*.tph)|*.tph|All files (*.*)|*.*";
            if (fd.ShowDialog() == true)
            {
                TideInfo.Load(fd.FileName);
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            Website.UploadTide upload = new Website.UploadTide(TideInfo.Data);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ReadDB_Click(object sender, RoutedEventArgs e)
        {
            string mysql = (string)DbSettings.GetSetting("mysql");
            MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(mysql);
            mysql = mcsb.ConnectionString;
            using (MySqlConnection mcon = new MySqlConnection(mysql))
            {
                try
                {
                    mcon.Open();
                    using (MySqlCommand mcom = new MySqlCommand())
                    {
                        mcom.Connection = mcon;
                        mcom.Parameters.AddWithValue("start", (new DateTime(TideInfo.BaseYear, 1, 1)).AddMinutes(-10));
                        mcom.Parameters.AddWithValue("end", new DateTime(TideInfo.BaseYear + 1, 1, 1));
                        mcom.CommandText = @"SELECT date, height, current, flow, tide 
FROM tidedata 
WHERE date BETWEEN @start AND @end
ORDER BY date";
                        using (MySqlDataAdapter adapt = new MySqlDataAdapter(mcom))
                        {
                            DataTable tides = new DataTable();
                            adapt.Fill(tides);
                            TideInfo.Data = tides;
                        }
                    }
                }
                finally
                {
                    mcon.Close();
                }
            }
        }

        private void DoFlowTide_Click(object sender, RoutedEventArgs e)
        {
            string flow = "E";  // assume ebbing
            DataTable d = TideInfo.Data;
            if (d.Rows.Count > 0 && d.Rows[0]["flow"] != DBNull.Value)
                flow = d.Rows[0]["flow"] as string;

            for (int i = 1; i < d.Rows.Count; i++)
            {
                if ((double)d.Rows[i]["height"] > (double)d.Rows[i - 1]["height"])
                    flow = "F";
                else if ((double)d.Rows[i]["height"] < (double)d.Rows[i - 1]["height"])
                    flow = "E";
                d.Rows[i]["flow"] = flow;
            }

            for (int i = 1; i < d.Rows.Count - 1; i++)
            {
                if ((string)d.Rows[i]["flow"] != (string)d.Rows[i + 1]["flow"])
                    if ((string)d.Rows[i]["flow"] == "E")
                        // potential low
                        d.Rows[i]["tide"] = "L";
                    else
                        d.Rows[i]["tide"] = "H";
            }

            DataRow[] tides = d.Select("tide is not null", "date");
            foreach (DataRow r in tides)
            {
                DateTime tide = r.Field<DateTime>("date");
                DataRow[] wibbles = d.Select(string.Format("date >= '{0}' and date <= '{1}' and tide is not null",
                    new object[] { tide.AddMinutes(-130), tide.AddMinutes(130) }), "date");
                if (wibbles.Length > 1)
                {
                    DataRow reference;
                    if (wibbles[0].Field<string>("tide") == "L")
                    {
                        reference = wibbles[0];
                        foreach (DataRow w in wibbles)
                        {
                            if (reference.Field<double>("height") > w.Field<double>("height"))
                                reference = w;
                        }
                        foreach (DataRow w in wibbles)
                        {
                            if (reference != w)
                                w["tide"] = DBNull.Value;
                        }
                    }
                }
            }

            TideInfo.Data = d;
        }
    }
}
