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
    public partial class Boat : Window
    {
        private int bid;
        public int Bid
        {
            get { return bid; }
        }

        private int? id;
        public int? Id
        {
            get { return id; }
        }

        public Boat(int b)
        {
            bid = b;
            InitializeComponent();
            if (bid != 0)
            {
                Db get = new Db("SELECT id, boatname, boatclass, sailno, dinghy, " +
                    "hulltype, open_handicap, handicap_status, " +
                    "rolling_handicap, small_cat_handicap_rating, engine_propeller, keel, deviations, boatmemo " +
                    "FROM boats " +
                    "WHERE bid = @bid");
                Hashtable p = new Hashtable();
                p["bid"] = Bid;
                Hashtable data = get.GetHashtable(p);

                if (data["id"] != DBNull.Value)
                {
                    id = (int)data["id"];
                    SetOwner();
                }
                boatName.Text = data["boatname"].ToString();
                boatClass.Text = data["boatclass"].ToString();
                sailNumber.Text = data["sailno"].ToString();
                dinghy.IsChecked = (bool)data["dinghy"];
                openHandicap.Text = data["open_handicap"].ToString();
                switch (data["handicap_status"].ToString())
                {
                    case "PY":
                        handicapStatus.SelectedValue = hstat_PY;
                        break;
                    case "SY":
                        handicapStatus.SelectedValue = hstat_SY;
                        break;
                    case "TN":
                        handicapStatus.SelectedValue = hstat_TN;
                        break;
                    case "CN":
                        handicapStatus.SelectedValue = hstat_CN;
                        break;
                    case "EN":
                        handicapStatus.SelectedValue = hstat_EN;
                        break;
                }
                rollingHandicap.Text = data["rolling_handicap"].ToString();
                smallCatHandicap.Text = data["small_cat_handicap_rating"].ToString();
                switch (data["engine_propeller"].ToString())
                {
                    case "OB":
                        engine.SelectedValue = OB;
                        break;
                    case "IB2":
                        engine.SelectedValue = IB2;
                        break;
                    case "IB3":
                        engine.SelectedValue = IB3;
                        break;
                    case "IBF":
                        engine.SelectedValue = IBF;
                        break;
                }
                switch (data["keel"].ToString())
                {
                    case "F":
                        keel.SelectedValue = keelF;
                        break;
                    case "D":
                        keel.SelectedValue = keelD;
                        break;
                    case "2K":
                        keel.SelectedValue = keel2K;
                        break;
                    case "3K":
                        keel.SelectedValue = keel3K;
                        break;
                }
                deviations.Text = data["deviations"].ToString();
                notes.Text = data["boatmemo"].ToString();
            }
            else
            {
            }
        }

        private void SetOwner()
        {
            Db get = new Db("SELECT firstname, surname " +
                "FROM people " +
                "WHERE id = @id");
            Hashtable p = new Hashtable();
            p["id"] = Id.Value;
            Hashtable owner = get.GetHashtable(p);
            BoatOwner.Text = (owner["firstname"].ToString() + " " +
                owner["surname"].ToString()).Trim();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            if (boatName.Text.Trim() == "")
            {
                MessageBox.Show("Boat name required", "Input Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.DialogResult = true;
            Hashtable p = new Hashtable();

            if (id != null)
                p["id"] = id.Value;
            else
                p["id"] = DBNull.Value;

            p["boatname"] = boatName.Text;
            p["boatclass"] = boatClass.Text;
            p["sailno"] = sailNumber.Text;
            p["dngy"] = dinghy.IsChecked.Value;

            switch (hullType.Text)
            {
                case "Catamaran (C)":
                    p["h"] = "C";
                    break;
                case "Open keelboat (K)":
                    p["h"] = "K";
                    break;
                default:
                    p["h"] = DBNull.Value;
                    break;
            }

            int hcap;
            if (openHandicap.Text != "")
            {
                if (Int32.TryParse(openHandicap.Text, out hcap))
                {
                    p["ohp"] = openHandicap.Text;
                }
                else
                {
                    MessageBox.Show(this, "Open handicap must be an integer", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                p["ohp"] = DBNull.Value;
            }

            if (rollingHandicap.Text != "")
            {
                if (Int32.TryParse(rollingHandicap.Text, out hcap))
                {
                    p["rhp"] = rollingHandicap.Text;
                }
                else
                {
                    MessageBox.Show(this, "Rolling handicap must be an integer", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                p["rhp"] = DBNull.Value;
            }

            switch (handicapStatus.Text)
            {
                case "Primary Yardstick (PY)":
                    p["ohstat"] = "PY";
                    break;
                case "Secondary Yardstick (SY)":
                    p["ohstat"] = "SY";
                    break;
                case "Tertiary Number (TN)":
                    p["ohstat"] = "TN";
                    break;
                case "Club Number (CN)":
                    p["ohstat"] = "CN";
                    break;
                case "Recorded Number (RN)":
                    p["ohstat"] = "RN";
                    break;
                case "Experimental Number (EN)":
                    p["ohstat"] = "EN";
                    break;
            }

            Double schr;
            if (smallCatHandicap.Text != "")
            {
                if (Double.TryParse(smallCatHandicap.Text, out schr))
                {
                    p["schr"] = smallCatHandicap.Text;
                }
                else
                {
                    MessageBox.Show(this, "Rolling handicap must be an integer", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                p["schr"] = DBNull.Value;
            }

            switch (engine.Text)
            {
                case "Outboard (OB)":
                    p["eng"] = "OB";
                    break;
                case "2 Blade (IB2)":
                    p["eng"] = "IB2";
                    break;
                case "3 Blade (IB3)":
                    p["eng"] = "IB3";
                    break;
                case "Folding (IBF)":
                    p["eng"] = "IBF";
                    break;
                default:
                    p["eng"] = DBNull.Value;
                    break;
            }
            switch (keel.Text)
            {
                case "Central Keel (F)":
                    p["kl"] = "F";
                    break;
                case "Adjustable Drop (D)":
                    p["kl"] = "D";
                    break;
                case "Twin bilge (2K)":
                    p["kl"] = "2K";
                    break;
                case "Central and bilge (3K)":
                    p["kl"] = "3K";
                    break;
                default:
                    p["kl"] = DBNull.Value;
                    break;
            }
            p["deviations"] = deviations.Text;
            p["boatmemo"] = notes.Text;
            p["bid"] = Bid;
            Db save;
            if (Bid == 0)
            {
                save = new Db("INSERT INTO boats " +
                        "(id, boatname, boatclass, sailno, dinghy, hulltype, open_handicap, " +
                        "handicap_status, rolling_handicap, small_cat_handicap_rating, " +
                        "engine_propeller, keel, deviations, boatmemo) " +
                        "VALUES (@id, @boatname, @boatclass, @sailno, @dngy, @h, @ohp, @ohstat, @rhp, @schr, @eng, @kl, @deviations, @boatmemo)");
                bid = save.GetNextIdentity("boats", "bid");
            }
            else
                save = new Db("UPDATE boats " +
                        "SET id = @id, " + 
                        "boatname = @boatname, " +
                        "boatclass = @boatclass, " +
                        "sailno = @sailno, " +
                        "dinghy = @dngy, " +
                        "hulltype = @h, " +
                        "open_handicap = @ohp, " +
                        "handicap_status = @ohstat, " +
                        "rolling_handicap = @rhp, " +
                        "small_cat_handicap_rating = @schr, " +
                        "engine_propeller = @eng, " +
                        "keel = @kl, " +
                        "deviations = @deviations, " +
                        "boatmemo = @boatmemo " +
                        "WHERE bid = @bid");
            save.ExecuteNonQuery(p);
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Bid == 0)
            {
                object o = DbSettings.GetSetting("topseed");
                if (o != null)
                {
                    int topseed, nextval;
                    topseed = (int)o;

                    Db seed = new Db("");
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
            People ppl = new People(Id);
            ppl.ShowDialog();
            if (Id.Value != ppl.Id)
            {
                id = ppl.Id;
                SetOwner();
            }
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

                boatClass.Text = data["class_name"].ToString();
                openHandicap.Text = data["number"].ToString();
                if (rollingHandicap.Text == "")
                    rollingHandicap.Text = data["number"].ToString();
                switch (data["status"].ToString())
                {
                    case "P":
                        handicapStatus.SelectedValue = hstat_PY;
                        break;
                    case "S":
                        handicapStatus.SelectedValue = hstat_SY;
                        break;
                    case "C":
                        handicapStatus.SelectedValue = hstat_CN;
                        break;
                    case "R":
                        handicapStatus.SelectedValue = hstat_RN;
                        break;
                    case "E":
                        handicapStatus.SelectedValue = hstat_EN;
                        break;
                }

                if (data["engine"] != DBNull.Value)
                {
                    switch (data["engine"].ToString())
                    {
                        case "OB":
                            engine.SelectedValue = OB;
                            break;
                        case "IB2":
                            engine.SelectedValue = IB2;
                            break;
                        case "IB3":
                            engine.SelectedValue = IB3;
                            break;
                        case "IBF":
                            engine.SelectedValue = IBF;
                            break;
                    }
                }
                else
                    engine.SelectedIndex = 0;

                if (data["keel"] != DBNull.Value)
                {
                    switch (data["keel"] as int?)
                    {
                        case 1:
                            keel.SelectedValue = keelF;
                            break;
                        case 2:
                            keel.SelectedValue = keel2K;
                            break;
                        case 3:
                            keel.SelectedValue = keel3K;
                            break;
                    }
                }
            }
        }
    }
}
