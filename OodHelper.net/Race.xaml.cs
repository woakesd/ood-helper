using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
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
using System.Windows.Markup;
using System.Xml;

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

        public RaceData raceData;
        Hashtable data;

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
                data = get.GetHashtable(p);
                raceData = new RaceData(data);
                this.DataContext = raceData;

                //startDate.SelectedDate = data["start_date"] as DateTime?;
                //startDate.DisplayDate = startDate.SelectedDate.Value;
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
            string msg = string.Empty;

            if (Validation.GetErrors(timeLimitFixedTime).Count > 0)
                msg += Validation.GetErrors(timeLimitFixedTime)[0].ErrorContent.ToString() + "\n";
            if (Validation.GetErrors(startTime).Count > 0)
                msg += Validation.GetErrors(timeLimitFixedTime)[0].ErrorContent.ToString() + "\n";
            if (startDate.SelectedDate == null || startTime.Text == string.Empty)
                msg += "Start date and time required\n";
            if (timeFixedRadio.IsChecked.Value && (timeLimitFixedDate.SelectedDate == null || timeLimitFixedTime.Text == string.Empty))
                msg += "Time limit date and time required if fixed time limit is selected\n";
            if (timeDeltaRadio.IsChecked.Value && timeLimitDelta.Text == string.Empty)
                msg += "Race length must be specified if time difference is specified\n";

            if (msg != string.Empty)
            {
                MessageBox.Show(msg, "Input not valid", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            this.DialogResult = true;
            Hashtable p = new Hashtable();

            p["start_date"] = startDate.SelectedDate.Value + TimeSpan.Parse(startTime.Text);
            if (timeFixedRadio.IsChecked.Value)
            {
                p["time_limit_type"] = "F";
                p["time_limit_delta"] = DBNull.Value;
                if (timeLimitFixedDate.SelectedDate != null && timeLimitFixedTime.Text != string.Empty)
                    p["time_limit_fixed"] = timeLimitFixedDate.SelectedDate.Value + TimeSpan.Parse(timeLimitFixedTime.Text);
                else
                    p["time_limit_fixed"] = DBNull.Value;
            }
            else if (timeDeltaRadio.IsChecked.Value)
            {
                p["time_limit_type"] = "D";
                if (timeLimitDelta.Text != string.Empty)
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
            if (extension.Text != string.Empty)
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
            if (timeLimitDelta.Text != string.Empty)
            {
                TimeSpan tlDelta;
                if (timeLimitDelta.Text.Length > 5)
                    tlDelta = TimeSpan.ParseExact(timeLimitDelta.Text, "d\\.hh\\:mm", null);
                else
                    tlDelta = TimeSpan.ParseExact(timeLimitDelta.Text, "hh\\:mm", null);
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
                if (x > new TimeSpan(1, 0, 0, 0))
                    timeLimitDelta.Text = x.ToString("d\\.hh\\:mm");
                else
                    timeLimitDelta.Text = x.ToString("hh\\:mm");
                timeLimitFixed.Visibility = System.Windows.Visibility.Collapsed;
                timeLimitDelta.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void startDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timeFixedRadio.IsChecked.Value)
            {
                DateTime? oldStart = data["start_date"] as DateTime?;
                if (oldStart != null && startDate.SelectedDate != null && timeLimitFixedDate != null)
                    timeLimitFixedDate.SelectedDate += startDate.SelectedDate.Value - oldStart.Value.Date;
                else
                    timeLimitFixedDate = startDate;
            }
        }

        private void timeLimitFixedDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timeLimitFixedDate.SelectedDate != null)
            {
                if (startDate.SelectedDate == null)
                    startDate.SelectedDate = timeLimitFixedDate.SelectedDate;
                if (timeLimitFixedDate.SelectedDate.Value < startDate.SelectedDate.Value)
                {
                    timeLimitFixedDate.SelectedDate = startDate.SelectedDate;
                    MessageBox.Show("Time limit date must be same or later than start date",
                        "Time Limit validation failure", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class RaceData: INotifyPropertyChanged
    {
        public Hashtable data;

        public RaceData(Hashtable d)
        {
            data = d;
        }

        public DateTime? StartDateDate
        {
            get
            {
                if (data["start_date"] != DBNull.Value)
                    return (data["start_date"] as DateTime?).Value.Date;
                return null;
            }

            set
            {
                if (value.HasValue)
                    data["start_date"] = value.Value;
                else
                    data["start_date"] = DBNull.Value;
                OnPropertyChanged("StartDateDate");
            }
        }
        public string TimeLimitFixedTime
        {
            get
            {
                if (data["time_limit_fixed"] != DBNull.Value)
                    return (data["time_limit_fixed"] as DateTime?).Value.ToString("HH:mm");
                return string.Empty;
            }

            set
            {
                if (data["time_limit_fixed"] != DBNull.Value)
                    try
                    {
                        (data["time_limit_fixed"] as DateTime?).Value.Date.Add(TimeSpan.ParseExact(value, "h\\:mm", null));
                        OnPropertyChanged("TimeLimitFixedTime");
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException("Time limit time format must be like 12:50");
                    }
                else
                    throw new ArgumentException("Time limit date must be selected first");
            }
        }

        public string StartDateTime
        {
            get
            {
                if (data["start_date"] != DBNull.Value)
                    return (data["start_date"] as DateTime?).Value.ToString("HH:mm");
                return string.Empty;
            }

            set
            {
                if (data["start_date"] != DBNull.Value)
                    try
                    {
                        (data["start_date"] as DateTime?).Value.Date.Add(TimeSpan.ParseExact(value, "h\\:mm", null));
                        OnPropertyChanged("StartDateTime");
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Start time format must be like 12:50");
                    }
                else
                    throw new ArgumentException("Start date must be selected first");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
