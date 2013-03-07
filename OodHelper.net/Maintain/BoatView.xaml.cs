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
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Linq;
using OodHelper.Helpers;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Boat.xaml
    /// </summary>
    public partial class BoatView : Window
    {
        public int Bid 
        {
            get 
            {
                BoatModel bm = DataContext as BoatModel;
                if (bm != null)
                    return bm.Bid.HasValue ? bm.Bid.Value : 0;
                return 0;
            } 
        }

        public BoatView(int b)
        {
            InitializeComponent();

            BoatModel bm = new BoatModel(b);
            DataContext = bm;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            //
            // If a user changes the content of a text box and then hits return
            // this trigger is fired without the text box firing it's lost focus
            // trigger, so we need to update the source for the text box by hand.
            //
            VisualHelper.UpdateTextBoxSources(this);

            BoatModel dc = DataContext as BoatModel;
            if (dc != null)
            {
                string msg;
                if ((msg = dc.CommitChanges()) == string.Empty)
                {
                    DialogResult = true;
                    Close();
                }
                else
                    MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Bid == 0)
            {
                object o = Settings.GetSetting("topseed");
                if (o != null)
                {
                    int topseed, nextval;
                    topseed = (int)o;

                    Db seed = new Db(string.Empty);
                    nextval = seed.GetNextIdentity("boats", "bid");

                    if (nextval > topseed)
                    {
                        MessageBox.Show("You need to get a new set of seed values", "Cannot add a new boat",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        this.DialogResult = false;
                        this.Close();
                    }
                }
            }
        }

        private void SelectPerson_Click(object sender, RoutedEventArgs e)
        {
            People ppl = new People(true, 0);
            if (ppl.ShowDialog() == true)
            {
                BoatModel dc = DataContext as BoatModel;
                if (!dc.Id.HasValue || ppl.Id.HasValue && dc.Id.Value != ppl.Id)
                {
                    dc.Id = ppl.Id;
                }
            }
        }

        private void SelectClass_Click(object sender, RoutedEventArgs e)
        {
            SelectClass cls = new SelectClass();
            if (cls.ShowDialog().Value)
            {
                BoatModel dc = DataContext as BoatModel;
                if (dc != null)
                {
                    Hashtable p = new Hashtable();
                    p["id"] = cls.Id;
                    Db hdb = new Db("SELECT * FROM portsmouth_numbers WHERE id = @id");
                    Hashtable data = hdb.GetHashtable(p);

                    dc.BoatClass = data["class_name"].ToString();
                    dc.OpenHandicap = data["number"].ToString();
                    if (dc.RollingHandicap == string.Empty)
                        dc.RollingHandicap = data["number"].ToString();
                    switch (data["status"].ToString())
                    {
                        case "P":
                            dc.HandicapStatus = "PY";
                            break;
                        case "S":
                            dc.HandicapStatus = "SY";
                            break;
                        case "C":
                            dc.HandicapStatus = "CN";
                            break;
                        case "R":
                            dc.HandicapStatus = "RN";
                            break;
                        case "E":
                            dc.HandicapStatus = "TN";
                            break;
                    }

                    if (data["engine"] != DBNull.Value)
                    {
                        dc.EnginePropeller = data["engine"].ToString();
                    }
                    else
                        dc.EnginePropeller = "";

                    if (data["keel"] != DBNull.Value)
                    {
                        switch (data["keel"] as int?)
                        {
                            case 1:
                                dc.Keel = "F";
                                break;
                            case 2:
                                dc.Keel = "2K";
                                break;
                            case 3:
                                dc.Keel = "3K";
                                break;
                        }
                    }
                }
            }
        }
    }
}
