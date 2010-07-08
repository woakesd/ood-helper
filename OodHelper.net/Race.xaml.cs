using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Linq;
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

        public Race(int r)
        {
            rid = r;
            InitializeComponent();
            if (rid != 0)
            {
                Results ld = new Results();
                Calendar raceData = ld.Calendar.Single(c => c.rid == r);
                this.DataContext = raceData;

                timeLimitFixedDate.BlackoutDates.Add(new CalendarDateRange(DateTime.MinValue, raceData.start_date.Value.AddDays(-1)));

                switch (raceData.time_limit_type)
                {
                    case "F":
                        timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
                        timeFixedRadio.IsChecked = true;
                        break;
                    case "D":
                        timeLimitFixed.Visibility = System.Windows.Visibility.Collapsed;
                        timeDeltaRadio.IsChecked = true;
                        break;
                    default:
                        timeLimitFixed.Visibility = System.Windows.Visibility.Collapsed;
                        timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                }
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
            Calendar raceData = DataContext as Calendar;

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

            p["start_date"] = raceData.start_date;
            if (timeFixedRadio.IsChecked.Value)
            {
                p["time_limit_type"] = "F";
                p["time_limit_delta"] = DBNull.Value;
                if (raceData.time_limit_fixed.HasValue)
                    p["time_limit_fixed"] = raceData.time_limit_fixed;
                else
                    p["time_limit_fixed"] = DBNull.Value;
            }
            else if (timeDeltaRadio.IsChecked.Value)
            {
                p["time_limit_type"] = "D";
                if (timeLimitDelta.Text != string.Empty)
                    p["time_limit_delta"] = raceData.time_limit_delta;
                else
                    p["time_limit_delta"] = DBNull.Value;
                p["time_limit_fixed"] = DBNull.Value;
            }
            p["class"] = raceData.calendar_class;
            p["event"] = raceData.calendar_event;
            p["price_code"] = raceData.price_code;
            p["course"] = raceData.course;
            p["ood"] = raceData.ood;
            p["venue"] = raceData.venue;
            p["average_lap"] = raceData.average_lap;
            p["timegate"] = raceData.timegate;
            p["handicapping"] = raceData.handicapping;
            p["visitors"] = raceData.visitors;
            p["flag"] = raceData.flag;
            p["extension"] = raceData.extension;
            p["memo"] = raceData.memo;
            p["raced"] = raceData.raced;
            p["approved"] = raceData.approved;
            p["is_race"] = raceData.is_race;
            p["rid"] = Rid;

            Db save;
            if (Rid == 0)
            {
                save = new Db("INSERT INTO calendar " +
                        "([start_date], [time_limit_type], [time_limit_delta], [time_limit_fixed], [class], [event], [start], [price_code], [course], [ood], [venue], [average_lap], [timegate], [handicapping], [visitors], [flag], [extension], [memo], [raced], [approved], [is_race]) " +
                        "VALUES (@start_date, @time_limit_type, @time_limit_delta, @time_limit_fixed, @class, @event, @start, @price_code, @course, @ood, @venue, @average_lap, @timegate, @handicapping, @visitors, @flag, @extension, @memo, @raced, @approved, @is_race)");
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
                    tlDelta = TimeSpan.ParseExact(timeLimitDelta.Text, "d\\ hh\\:mm", null);
                else
                    tlDelta = TimeSpan.ParseExact(timeLimitDelta.Text, "h\\:mm", null);
                DateTime start = startDate.SelectedDate.Value + TimeSpan.Parse(startTime.Text);
                DateTime tlFixed = start + tlDelta;
                timeLimitFixedDate.SelectedDate = tlFixed.Date;
                timeLimitFixedTime.Text = tlFixed.ToString("HH:mm");
                timeLimitFixed.Visibility = System.Windows.Visibility.Visible;
                timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void timeNoLimitRadio_Checked(object sender, RoutedEventArgs e)
        {
            timeLimitFixed.Visibility = System.Windows.Visibility.Collapsed;
            timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void timeDeltaRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (timeLimitFixedDate.SelectedDate != null)
            {
                DateTime start = startDate.SelectedDate.Value + TimeSpan.Parse(startTime.Text);
                DateTime tlFixed = timeLimitFixedDate.SelectedDate.Value + TimeSpan.Parse(timeLimitFixedTime.Text);
                TimeSpan x = tlFixed - start;
                if (x > new TimeSpan(1, 0, 0, 0))
                    timeLimitDelta.Text = x.ToString("d\\ hh\\:mm");
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
                Calendar raceData = DataContext as Calendar;
                if (raceData.start_date != null && startDate.SelectedDate != null && timeLimitFixedDate != null)
                    timeLimitFixedDate.SelectedDate += startDate.SelectedDate.Value - raceData.start_date.Value.Date;
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

}
