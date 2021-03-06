﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Boat.xaml
    /// </summary>
    public partial class RaceEdit : Window
    {
        private int rid;
        public int Rid
        {
            get { return rid; }
        }

        Calendar raceData;

        public RaceEdit(int r)
        {
            try
            {
                rid = r;
                InitializeComponent();
                if (rid != 0)
                {
                    using (Db _conn = new Db("SELECT * FROM calendar WHERE rid = @rid"))
                    {
                        Hashtable _para = new Hashtable();
                        _para["rid"] = rid;
                        Hashtable _data = _conn.GetHashtable(_para);
                        raceData = new Calendar()
                        {
                            calendar_event = _data["event"] as string,
                            calendar_class = _data["class"] as string,
                            flag = _data["flag"] as string,
                            course = _data["course"] as string,
                            start_date = _data["start_date"] as DateTime?,
                            time_limit_type = _data["time_limit_type"] as string,
                            time_limit_fixed = _data["time_limit_fixed"] as DateTime?,
                            time_limit_delta = _data["time_limit_delta"] as int?,
                            extension = _data["extension"] as int?,
                            ood = _data["ood"] as string,
                            venue = _data["venue"] as string,
                            standard_corrected_time = _data["standard_corrected_time"] as double?,
                            price_code =_data["price_code"] as string,
                            handicapping = _data["handicapping"] as string,
                            racetype = _data["racetype"] as string,
                            visitors = _data["visitors"] as int?,
                            raced = _data["raced"] as bool?,
                            approved = _data["approved"] as bool?,
                            is_race = _data["is_race"] as bool?,
                        };
                        //raceData = ld.Calendar.Single(c => c.rid == r);

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
                }
                else
                {
                    raceData = new Calendar();
                    timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
                    timeFixedRadio.IsChecked = true;
                    raceData.racetype = "";
                    raceData.is_race = true;
                    raceData.raced = false;
                    raceData.approved = false;
                    raceData.time_limit_type = "F";
                }
                DataContext = raceData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
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
                msg += Validation.GetErrors(startTime)[0].ErrorContent.ToString() + "\n";
            if (Validation.GetErrors(timeLimitDelta).Count > 0)
                msg += Validation.GetErrors(timeLimitDelta)[0].ErrorContent.ToString() + "\n";
            if (Validation.GetErrors(extension).Count > 0)
                msg += Validation.GetErrors(extension)[0].ErrorContent.ToString() + "\n";

            if (raceData.is_race.Value)
            {
                if (raceData.calendar_event == null || raceData.calendar_event == string.Empty)
                    msg += "Event name must be provided\n";
                if (raceData.calendar_class == null || raceData.calendar_class == string.Empty)
                    msg += "Class must be provided\n";
                if (!raceData.start_date.HasValue)
                    msg += "Start date and time must be provided\n";
                switch (raceData.time_limit_type)
                {
                    case "F":
                        if (!raceData.time_limit_fixed.HasValue)
                            msg += "Fixed time limit date and time must be provided\n";
                        if (raceData.time_limit_fixed < raceData.start_date)
                            msg += "Fixed time limit must be after start\n";
                        if (!raceData.extension.HasValue)
                            msg += "Extension must be provided\n";
                        break;
                    case "D":
                        if (!raceData.time_limit_delta.HasValue)
                            msg += "Time limit delta must be provided\n";
                        if (!raceData.extension.HasValue)
                            msg += "Extension must be provided\n";
                        break;
                }
            }

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
            else
            {
                p["time_limit_type"] = DBNull.Value;
                p["time_limit_delta"] = DBNull.Value;
                p["time_limit_fixed"] = DBNull.Value;
            }
            p["class"] = raceData.calendar_class;
            p["event"] = raceData.calendar_event;
            p["price_code"] = raceData.price_code;
            p["course"] = raceData.course;
            p["ood"] = raceData.ood;
            p["venue"] = raceData.venue;
            p["racetype"] = raceData.racetype;
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
                        "([start_date], [time_limit_type], [time_limit_delta], [time_limit_fixed], [class], [event], [price_code], [course], [ood], [venue], [racetype], [handicapping], [visitors], [flag], [extension], [memo], [raced], [approved], [is_race]) " +
                        "VALUES (@start_date, @time_limit_type, @time_limit_delta, @time_limit_fixed, @class, @event, @price_code, @course, @ood, @venue, @racetype, @handicapping, @visitors, @flag, @extension, @memo, @raced, @approved, @is_race)");
                rid = save.GetNextIdentity("calendar");
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
                        ", racetype = @racetype " +
                        ", handicapping = @handicapping " +
                        ", visitors = @visitors " +
                        ", flag = @flag " +
                        ", extension = @extension " +
                        ", memo = @memo " +
                        ", raced = @raced " +
                        ", is_race = @is_race" +
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
                Db hdb = new Db("SELECT * FROM portsmouth_numbers WHERE id = @id");
                Hashtable data = hdb.GetHashtable(p);

                raceClass.Text = data["class_name"].ToString();
            }
        }

        private void timeFixedRadio_Checked(object sender, RoutedEventArgs e)
        {
            raceData.time_limit_type = "F";
            if (timeLimitDelta.Text != string.Empty)
            {
                TimeSpan tlDelta = new TimeSpan(0, 0, raceData.time_limit_delta.Value);
                DateTime start = raceData.start_date.Value;
                DateTime tlFixed = start + tlDelta;
                timeLimitFixedDate.SelectedDate = tlFixed.Date;
                timeLimitFixedTime.Text = tlFixed.ToString("HH:mm");
            }
            timeLimitFixed.Visibility = System.Windows.Visibility.Visible;
            timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
            extension.Visibility = System.Windows.Visibility.Visible;
            ExtensionLabel.Visibility = System.Windows.Visibility.Visible;
        }

        private void timeNoLimitRadio_Checked(object sender, RoutedEventArgs e)
        {
            raceData.time_limit_type = "";
            timeLimitFixed.Visibility = System.Windows.Visibility.Collapsed;
            timeLimitDelta.Visibility = System.Windows.Visibility.Collapsed;
            extension.Visibility = System.Windows.Visibility.Collapsed;
            ExtensionLabel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void timeDeltaRadio_Checked(object sender, RoutedEventArgs e)
        {
            raceData.time_limit_type = "D";
            if (raceData.time_limit_fixed.HasValue && raceData.start_date.HasValue)
            {
                DateTime start = raceData.start_date.Value;
                DateTime tlFixed = raceData.time_limit_fixed.Value;
                TimeSpan x = tlFixed - start;
                raceData.time_limit_delta = (int) x.TotalSeconds;
            }
            timeLimitFixed.Visibility = System.Windows.Visibility.Collapsed;
            timeLimitDelta.Visibility = System.Windows.Visibility.Visible;
            extension.Visibility = System.Windows.Visibility.Visible;
            ExtensionLabel.Visibility = System.Windows.Visibility.Visible;
        }

        private void startDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timeFixedRadio.IsChecked.Value)
            {
                timeLimitFixedDate.SelectedDate = raceData.start_date_date;
            }
        }

        private void timeLimitFixedDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (raceData.time_limit_fixed_date.HasValue)
            {
                if (!raceData.start_date_date.HasValue)
                    raceData.start_date_date = raceData.time_limit_fixed.Value;
            }
        }
    }

}
