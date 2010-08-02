using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for EntrySheetSelector.xaml
    /// </summary>
    public partial class EntrySheetSelector : Window
    {
        public EntrySheetSelector()
        {
            InitializeComponent();
            Db c = new Db(@"SELECT 0 print_all_visible, 0 print_all, 0 [print], start_date, event, class
                FROM calendar
                WHERE is_race = 1
                AND raced = 0
                AND start_date BETWEEN @today
                AND DATEADD(DAY, 10, @today)
                ORDER BY start_date");
            Hashtable para = new Hashtable();
            para["today"] = DateTime.Today;
            DataTable d = c.GetData(para);
            DateTime? NextDate = null;

            if (d.Rows.Count > 0)
            {
                NextDate = d.Rows[0]["start_date"] as DateTime?;
                d.Rows[0]["print_all_visible"] = true;
                d.Rows[0]["print_all"] = true;
            }
            for (int i = 0; i < d.Rows.Count; i++)
            {
                DataRow p = null;
                if (i > 0) p = d.Rows[i-1];
                DataRow r = d.Rows[i];
                DateTime? start = r["start_date"] as DateTime?;
                if (start.HasValue)
                {
                    if (start.Value.Date == NextDate.Value.Date)
                        r["print"] = true;
                    else
                        r["print"] = false;
                    if (i > 0 && ((DateTime)r["start_date"]).Date != ((DateTime)p["start_date"]).Date)
                        r["print_all_visible"] = true;
                    else if (i > 0)
                        r["print_all_visible"] = false;
                }
            }
            Races.ItemsSource = d.DefaultView;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Include_All_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox;
            if (cb != null)
            {
                DataRowView rv = cb.DataContext as DataRowView;
                DataRow r = rv.Row;
                DataTable d = rv.DataView.Table;
                foreach (DataRow p in d.Rows)
                {
                    if (((DateTime)p["start_date"]).Date == ((DateTime)r["start_date"]).Date)
                        p["print"] = cb.IsChecked;
                }
            }
        }

        private void Include_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox;
            if (cb != null)
            {
                DataRowView rv = cb.DataContext as DataRowView;
                DataRow r = rv.Row;
                DataTable d = rv.DataView.Table;
                for (int i = 0; i < d.Rows.Count; i++)
                {
                    DataRow p = d.Rows[i];
                    if (((DateTime)p["start_date"]).Date == ((DateTime)r["start_date"]).Date
                        && (int)p["print_all_visible"] == 1
                        && cb.IsChecked == false)
                        p["print_all"] = false;
                    if ((int)p["print_all_visible"] == 1)
                    {
                        bool allprint = true;
                        for (int j = i; j < d.Rows.Count; j++)
                        {
                            DataRow q = d.Rows[j];
                            if (((DateTime)q["start_date"]).Date == ((DateTime)p["start_date"]).Date &&
                                (int)q["print"] == 0)
                            {
                                allprint = false;
                                break;
                            }
                        }
                        p["print_all"] = allprint;
                    }
                }
            }
        }
    }
}
