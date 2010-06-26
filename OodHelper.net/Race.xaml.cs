using System;
using System.Collections;
using System.Collections.Generic;
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

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for Boat.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class Race : Window
    {
        private int rid;
        public int Rid
        {
            get { return rid; }
        }

        public Race(int r)
        {
            rid = r;
            InitializeComponent();
            if (rid != 0)
            {
                Db get = new Db("SELECT * " +
                    "FROM calendar " +
                    "WHERE rid = @rid");
                Hashtable p = new Hashtable();
                p["rid"] = Rid;
                Hashtable data = get.GetHashtable(p);

                startDate.SelectedDate = data["start_date"] as DateTime?;
                startDate.DisplayDate = startDate.SelectedDate.Value;
                startTime.Text = (data["start_date"] as DateTime?).Value.ToString("HH:mm");
                eventName.Text = data["event"] as string;
                raceClass.Text = data["class"] as string;
                eventName.Text = data["event"] as string;
                course.Text = data["course"] as string;
                ood.Text = data["ood"] as string;
                venue.Text = data["venue"] as string;
                flag.Text = data["flag"] as string;
                switch (data["time_limit_type"].ToString())
                {
                    case "F":
                        timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
                        timeLimitFixedDate.SelectedDate = data["time_limit_fixed"] as DateTime?;
                        if (timeLimitFixedDate.SelectedDate != null)
                            timeLimitFixedTime.Text = (data["time_limit_fixed"] as DateTime?).Value.ToString("HH:mm");
                        timeFixedRadio.IsChecked = true;
                        break;
                    case "D":
                        timeLimitFixed.Visibility = System.Windows.Visibility.Collapsed;
                        TimeSpan t = new TimeSpan(0, 0, (int)data["time_limit_delta"]);
                        timeLimitDelta.Text = t.ToString("hh\\:mm");
                        timeDeltaRadio.IsChecked = true;
                        break;
                    default:
                        timeLimitFixed.Visibility = System.Windows.Visibility.Collapsed;
                        timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                }
                if (data["extension"] != DBNull.Value)
                    extension.Text = (new TimeSpan(0, 0, (int)data["extension"])).ToString("hh\\:mm");
                memo.Text = data["memo"] as string;
                switch (data["price_code"] as string)
                {
                    case null:
                        priceCode.SelectedItem = PC_None;
                        break;
                    case "a":
                        priceCode.SelectedItem = PC_A;
                        break;
                    case "b":
                        priceCode.SelectedItem = PC_B;
                        break;
                    case "e":
                        priceCode.SelectedItem = PC_E;
                        break;
                    case "f":
                        priceCode.SelectedItem = PC_F;
                        break;
                    case "y":
                        priceCode.SelectedItem = PC_Y;
                        break;
                    case "z":
                        priceCode.SelectedItem = PC_Z;
                        break;
                }
                averageLap.IsChecked = (data["average_lap"] != DBNull.Value && (bool)data["average_lap"]) ? true : false;
                timegate.IsChecked = (data["timegate"] != DBNull.Value && (bool)data["timegate"]) ? true : false;
                switch (data["handicapping"] as string)
                {
                    case null:
                        hc.SelectedItem = HC_None;
                        break;
                    case "o":
                        hc.SelectedItem = HC_Open;
                        break;
                    case "r":
                        hc.SelectedItem = HC_Rolling;
                        break;
                    case "s":
                        hc.SelectedItem = HC_SCHR;
                        break;
                }
                visitors.Text = data["visitors"].ToString();
                raced.IsChecked = (data["raced"] != DBNull.Value && (bool)data["raced"]) ? true : false;
                approved.IsChecked = (data["approved"] != DBNull.Value && (bool)data["approved"]) ? true : false;
                standardCorrectedTime.Text = Common.HMS((double)data["standard_corrected_time"]);
            }
            else
            {
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            /*if (boatName.Text.Trim() == "")
            {
                MessageBox.Show("Boat name required", "Input Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }*/

            this.DialogResult = true;
            Hashtable p = new Hashtable();

            p["start_date"] = startDate.SelectedDate.Value + TimeSpan.Parse(startTime.Text);
            if (timeFixedRadio.IsChecked.Value)
            {
                p["time_limit_type"] = "F";
                p["time_limit_delta"] = DBNull.Value;
                if (timeLimitFixedDate.SelectedDate != null && timeLimitFixedTime.Text != "")
                    p["time_limit_fixed"] = timeLimitFixedDate.SelectedDate.Value + TimeSpan.Parse(timeLimitFixedTime.Text);
                else
                    p["time_limit_fixed"] = DBNull.Value;
            }
            else if (timeDeltaRadio.IsChecked.Value)
            {
                p["time_limit_type"] = "D";
                if (timeLimitDelta.Text != "")
                    p["time_limit_delta"] = Math.Round(TimeSpan.Parse(timeLimitDelta.Text).TotalSeconds);
                else
                    p["time_limit_delta"] = DBNull.Value;
                p["time_limit_fixed"] = DBNull.Value;
            }
            p["class"] = raceClass.Text;
            p["event"] = eventName.Text;
            p["start"] = startTime.Text;
            if (((ComboBoxItem)priceCode.SelectedItem).Tag != null)
                p["price_code"] = ((ComboBoxItem)priceCode.SelectedItem).Tag.ToString();
            else
                p["price_code"] = DBNull.Value;
            p["course"] = course.Text;
            p["ood"] = ood.Text;
            p["venue"] = venue.Text;
            p["average_lap"] = averageLap.IsChecked;
            p["timegate"] = timegate.IsChecked;
            if (((ComboBoxItem)hc.SelectedItem).Tag != null)
                p["handicapping"] = ((ComboBoxItem)hc.SelectedItem).Tag.ToString();
            else
                p["handicapping"] = DBNull.Value;
            p["visitors"] = visitors.Text;
            p["flag"] = flag.Text;
            if (extension.Text != "")
                p["extension"] = Math.Round(TimeSpan.Parse(extension.Text).TotalSeconds);
            else
                p["extension"] = DBNull.Value;
            p["memo"] = memo.Text;
            p["raced"] = raced.IsChecked;
            p["approved"] = approved.IsChecked;
            p["rid"] = Rid;

            Db save;
            if (Rid == 0)
            {
                save = new Db("INSERT INTO calendar " +
                        "([start_date], [time_limit_type], [time_limit_delta], [time_limit_fixed], [class], [event], [start], [price_code], [course], [ood], [venue], [average_lap], [timegate], [handicapping], [visitors], [flag], [extension], [memo], [raced], [approved]) " +
                        "VALUES (@start_date, @time_limit_type, @time_limit_delta, @time_limit_fixed, @class, @event, @start, @price_code, @course, @ood, @venue, @average_lap, @timegate, @handicapping, @visitors, @flag, @extension, @memo, @raced, @approved)");
                rid = save.GetNextIdentity("calendar", "rid");
            }
            else
                save = new Db("UPDATE calendar " +
                        "SET start_date = @start_date " +
                        ", time_limit_type = @time_limit_type " +
                        ", time_limit_fixed = @time_limit_fixed " +
                        ", time_limit_delta = @time_limit_delta " +
                        ", class = @class " +
                        ", event = @event " +
                        ", price_code = @price_code " +
                        ", course = @course " + 
                        ", ood = @ood " +
                        ", venue = @venue " +
                        ", average_lap = @average_lap " +
                        ", timegate = @timegate " +
                        ", handicapping = @handicapping " +
                        ", visitors = @visitors " +
                        ", flag = @flag " +
                        ", extension = @extension " +
                        ", memo = @memo " +
                        ", raced = @raced " +
                        ", approved = @approved " +
                        "WHERE rid = @rid");
            save.ExecuteNonQuery(p);
            this.Close();
        }

        private void SelectClass_Click(object sender, RoutedEventArgs e)
        {
            SelectClass cls = new SelectClass();
            if (cls.ShowDialog().Value)
            {
                Hashtable p = new Hashtable();
                p["id"] = cls.Id;
                HandicapDb hdb = new HandicapDb("SELECT * FROM portsmouth_numbers WHERE id = @id");
                Hashtable data = hdb.GetHashtable(p);

                raceClass.Text = data["class_name"].ToString();
            }
        }

        private void timeFixedRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (timeLimitDelta.Text != "")
            {
                TimeSpan tlDelta = TimeSpan.Parse(timeLimitDelta.Text);
                DateTime start = startDate.SelectedDate.Value + TimeSpan.Parse(startTime.Text);
                DateTime tlFixed = start + tlDelta;
                timeLimitFixedDate.SelectedDate = tlFixed.Date;
                timeLimitFixedTime.Text = tlFixed.ToString("HH:mm");
                timeLimitFixed.Visibility = System.Windows.Visibility.Visible;
                timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void timeDeltaRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (timeLimitFixedDate.SelectedDate != null)
            {
                DateTime start = startDate.SelectedDate.Value + TimeSpan.Parse(startTime.Text);
                DateTime tlFixed = timeLimitFixedDate.SelectedDate.Value + TimeSpan.Parse(timeLimitFixedTime.Text);
                TimeSpan x = tlFixed - start;
                timeLimitDelta.Text = Math.Truncate(x.TotalHours).ToString("0#") + ":" + x.Minutes.ToString("0#");
                timeLimitFixed.Visibility = System.Windows.Visibility.Collapsed;
                timeLimitDelta.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
