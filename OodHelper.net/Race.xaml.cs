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

                date.SelectedDate = data["date"] as DateTime?;
                date.DisplayDate = date.SelectedDate.Value;
                eventName.Text = data["event"] as string;
                raceClass.Text = data["class"] as string;
                eventName.Text = data["event"] as string;
                start.Text = data["start"] as string;
                course.Text = data["course"] as string;
                ood.Text = data["ood"] as string;
                venue.Text = data["venue"] as string;
                flag.Text = data["flag"] as string;
                timeLimit.Text = data["timelimit"] as string;
                extension.Text = data["extension"] as string;
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

            p["date"] = date.SelectedDate;
            p["class"] = raceClass.Text;
            p["event"] = eventName.Text;
            p["start"] = start.Text;
            p["price_code"] = ((ComboBoxItem) priceCode.SelectedItem).Tag.ToString();
            p["course"] = course.Text;
            p["ood"] = ood.Text;
            p["venue"] = venue.Text;
            p["average_lap"] = averageLap.IsChecked;
            p["timegate"] = timegate.IsChecked;
            p["handicapping"] = ((ComboBoxItem) hc.SelectedItem).Tag.ToString();
            p["visitors"] = visitors.Text;
            p["flag"] = flag.Text;
            p["timelimit"] = timeLimit.Text;
            p["extension"] = extension.Text;
            p["memo"] = memo.Text;
            p["raced"] = raced.IsChecked;
            p["approved"] = approved.IsChecked;
            p["rid"] = Rid;

            Db save;
            if (Rid == 0)
            {
                save = new Db("INSERT INTO calendar " +
                        "([date], [class], [event], [start], [price_code], [course], [ood], [venue], [average_lap], [timegate], [handicapping], [visitors], [flag], [timelimit], [extension], [memo], [raced], [approved]) " +
                        "VALUES (@date, @class, @event, @start, @price_code, @course, @ood, @venue, @average_lap, @timegate, @handicapping, @visitors, @flag, @timelimit, @extension, @memo, @raced, @approved)");
                rid = save.GetNextIdentity("calendar", "rid");
            }
            else
                save = new Db("UPDATE calendar " +
                        "SET date = @date " +
                        ", class = @class " +
                        ", event = @event " +
                        ", start = @start " +
                        ", price_code = @price_code " +
                        ", course = @course " + 
                        ", ood = @ood " +
                        ", venue = @venue " +
                        ", average_lap = @average_lap " +
                        ", timegate = @timegate " +
                        ", handicapping = @handicapping " +
                        ", visitors = @visitors " +
                        ", flag = @flag " +
                        ", timelimit = @timelimit " +
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
    }
}
