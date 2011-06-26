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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for EntrySheets.xaml
    /// </summary>
    public partial class EntrySheet : Page
    {
        public EntrySheet(int rid)
        {
            InitializeComponent();
            Db c = new Db(@"SELECT event, class, start_date, ood, 
                time_limit_type, time_limit_fixed, time_limit_delta, extension
                FROM calendar
                WHERE rid = @rid");
            Hashtable p = new Hashtable();
            p["rid"] = rid;
            Hashtable d = c.GetHashtable(p);
            EventName.Text = d["event"] as string;
            ClassName.Text = d["class"] as string;
            DateTime? dt = d["start_date"] as DateTime?;
            StartDate.Text = dt.Value.ToString("dd MMM yyyy");
            StartTime.Text = dt.Value.ToString("HH:mm");
            switch (d["time_limit_type"] as string)
            {
                case "F":
                    Fixed.Visibility = Visibility.Visible;
                    Delta.Visibility = Visibility.Collapsed;
                    if (d["time_limit_fixed"] != DBNull.Value)
                        TimeLimit.Text = ((DateTime)d["time_limit_fixed"]).ToString("HH:mm");
                    break;
                case "D":
                    Fixed.Visibility = Visibility.Collapsed;
                    Delta.Visibility = Visibility.Visible;
                    if (d["time_limit_delta"] != DBNull.Value)
                        TimeLimit.Text = (new TimeSpan(0, 0, (int)d["time_limit_delta"])).ToString("hh\\:mm");
                    break;
                default:
                    Fixed.Visibility = Visibility.Collapsed;
                    Delta.Visibility = Visibility.Collapsed;
                    TimeLimit.Text = "No Time Limit";
                    break;
            }
            if (d["extension"] != DBNull.Value)
                Extension.Text = (new TimeSpan(0, 0, (int)d["extension"])).ToString("hh\\:mm");
            else
                Extension.Text = "No Extension";
            OOD.Text = d["ood"] as string;
        }
    }
}
