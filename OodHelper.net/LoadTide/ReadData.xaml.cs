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
using MySql.Data;
using MySql.Data.MySqlClient;

namespace OodHelper.LoadTide
{
    /// <summary>
    /// Interaction logic for ReadData.xaml
    /// </summary>
    public partial class ReadData : Window
    {
        public ReadData()
        {
            InitializeComponent();
            DataTable tides = new DataTable();
            tides.Columns.Add("Date", typeof(DateTime));
            tides.Columns.Add("Height", typeof(double));
            tides.Columns.Add("Current", typeof(double));

            DateTime currdate = DateTime.Today, date;
            TimeSpan time;
            double height;

            string[] td = File.ReadAllLines(@"C:\Users\Administrator\Documents\PEYC Website\at-rosyth-2011.txt");
            foreach (string t in td)
            {
                if (DateTime.TryParseExact(t, "d MMM yyyy dddd", null, System.Globalization.DateTimeStyles.None, out date))
                {
                    currdate = date;
                }
                else if (t.Length >= 12 && TimeSpan.TryParseExact(t.Substring(0, 5), "hh\\:mm", null, out time) && Double.TryParse(t.Substring(5, 7), out height))
                {
                    DataRow tr = tides.NewRow();
                    tr["date"] = currdate + time;
                    tr["height"] = height;
                    tides.Rows.Add(tr);
                }               
            }

            Db c = new Db(@"INSERT INTO tide ([date], [height], [current]) VALUES (@date, @height, @current)");
            Hashtable p = new Hashtable();
            for (int i = 1; i < tides.Rows.Count; i++)
            {
                tides.Rows[i]["current"] = Math.Round(((double)tides.Rows[i]["height"] - (double)tides.Rows[i - 1]["height"]) * 8.32, 1);
                p["date"] = tides.Rows[i]["date"];
                p["height"] = tides.Rows[i]["height"];
                p["current"] = tides.Rows[i]["current"];
                c.ExecuteNonQuery(p);
            }
            TideTable.ItemsSource = tides.DefaultView;
        }
    }
}
