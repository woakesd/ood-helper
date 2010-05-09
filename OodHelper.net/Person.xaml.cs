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
    public partial class Person : Window
    {
        private int id;
        public int Id
        {
            get { return id; }
        }

        public Person(int pid)
        {
            id = pid;
            InitializeComponent();
            if (Id != 0)
            {
                Db get = new Db("SELECT firstname, surname, address1, address2, address3, address4, " +
                    "postcode, hometel, mobile, worktel, email, club, member, manmemo " +
                    "FROM people " +
                    "WHERE id = @id");
                Hashtable p = new Hashtable();
                p["id"] = Id;
                Hashtable data = get.GetHashtable(p);

                FirstName.Text = data["firstname"].ToString();
                LastName.Text = data["surname"].ToString();
                Address1.Text = data["address1"].ToString();
                Address2.Text = data["address2"].ToString();
                Address3.Text = data["address3"].ToString();
                Address4.Text = data["address4"].ToString();
                Postcode.Text = data["postcode"].ToString();
                HomePhone.Text = data["hometel"].ToString();
                MobilePhone.Text = data["mobile"].ToString();
                WorkPhone.Text = data["worktel"].ToString();
                Email.Text = data["email"].ToString();
                Club.Text = data["club"].ToString();
                Membership.Text = data["member"].ToString();
                Notes.Text = data["manmemo"].ToString();
            }
            else
            {
                int bottomseed = 1, topseed = 1999, nextval = 1;
                object o = DbSettings.GetSetting("bottomseed");
                if (o != null) bottomseed = (int)o;
                o = DbSettings.GetSetting("topseed");
                if (o != null) topseed = (int)o;
                Db seed = new Db("SELECT MAX(id) " +
                    "FROM people " +
                    "WHERE id BETWEEN @bottomseed AND @topseed");
                Hashtable p = new Hashtable();
                p["topseed"] = topseed;
                p["bottomseed"] = bottomseed;
                o = seed.GetScalar(p);
                if (o != null) nextval = (int)o + 1;

                if (nextval > topseed)
                {
                    MessageBox.Show("You need to get a new set of seed values", "Cannot add a new person",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;
                    this.Close();
                }
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Hashtable p = new Hashtable();
            p["firstname"] = FirstName.Text;
            p["surname"] = LastName.Text;
            p["address1"] = Address1.Text;
            p["address2"] = Address2.Text;
            p["address3"] = Address3.Text;
            p["address4"] = Address4.Text;
            p["postcode"] = Postcode.Text;
            p["hometel"] = HomePhone.Text;
            p["mobile"] = MobilePhone.Text;
            p["worktel"] = WorkPhone.Text;
            p["email"] = Email.Text;
            p["club"] = Club.Text;
            p["member"] = Membership.Text;
            p["manmemo"] = Notes.Text;
            p["id"] = Id;
            Db save;
            if (Id == 0)
                save = new Db("INSERT INTO people " +
                    "(firstname, surname, address1, address2, address3, address4, " +
                    "postcode, hometel, mobile, worktel, email, club, member, manmemo) " +
                    "VALUES (@firstname, @surname, @address1, @address2, @address3, @address4, " +
                    "@postcode, @hometel, @mobile, @worktel, @email, @club, @member, @manmemo)");
            else
                save = new Db("UPDATE people " +
                        "SET firstname = @firstname, " +
                        "surname = @surname, " +
                        "address1 = @address1, " +
                        "address2 = @address2, " +
                        "address3 = @address3, " +
                        "address4 = @address4, " +
                        "postcode = @postcode, " +
                        "hometel = @hometel, " +
                        "mobile = @mobile, " +
                        "worktel = @worktel, " +
                        "email = @email, " +
                        "club = @club, " +
                        "member = @member, " +
                        "manmemo = @manmemo " +
                        "WHERE id = @id");
            save.ExecuteNonQuery(p);
            this.Close();
        }
    }
}
