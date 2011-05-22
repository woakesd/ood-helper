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

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for FamilyMember.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class FamilyMember : Window
    {
        private int mId;
        public int Id
        {
            get { return mId; }
        }

        private int mMainId;
        public int MainId
        {
            get { return mMainId; }
        }

        public FamilyMember(int id, int mainid)
        {
            InitializeComponent();
            mId = id;
            mMainId = mainid;
            if (Id != 0)
            {
                Db get = new Db("SELECT firstname, surname, " +
                    "hometel, mobile, worktel, email, manmemo " +
                    "FROM people " +
                    "WHERE id = @id");
                Hashtable p = new Hashtable();
                p["id"] = Id;
                Hashtable data = get.GetHashtable(p);

                ID.Text = Id.ToString();
                FirstName.Text = data["firstname"].ToString();
                LastName.Text = data["surname"].ToString();
                HomePhone.Text = data["hometel"].ToString();
                MobilePhone.Text = data["mobile"].ToString();
                WorkPhone.Text = data["worktel"].ToString();
                Email.Text = data["email"].ToString();
                Notes.Text = data["manmemo"].ToString();

                Crewing.ItemsSource = Person.BoatCrewFill(Id);
            }
            else
            {
                int topseed = 1999, nextval = 1;
                object o = DbSettings.GetSetting("topseed");
                if (o != null) topseed = (int)o;

                Db seed = new Db(string.Empty);
                nextval = seed.GetNextIdentity("people", "id");

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
            Hashtable p = new Hashtable();
            Db save;
            save = new Db("SELECT * " +
                "FROM people " +
                "WHERE id = @id");
            p["id"] = MainId;
            p = save.GetHashtable(p);
            
            p["firstname"] = FirstName.Text;
            p["surname"] = LastName.Text;
            p["hometel"] = HomePhone.Text;
            p["mobile"] = MobilePhone.Text;
            p["worktel"] = WorkPhone.Text;
            p["email"] = Email.Text;
            p["member"] = "Fmemb";
            p["manmemo"] = Notes.Text;
            p["main_id"] = MainId;
            p["id"] = Id;
            if (Id == 0)
            {
                save = new Db("INSERT INTO people " +
                    "(main_id, firstname, surname, address1, address2, address3, address4, " +
                    "postcode, hometel, mobile, worktel, email, club, member, manmemo) " +
                    "VALUES (@main_id, @firstname, @surname, @address1, @address2, @address3, @address4, " +
                    "@postcode, @hometel, @mobile, @worktel, @email, @club, @member, @manmemo)");
                save.ExecuteNonQuery(p);
            }
            else
            {
                save = new Db("UPDATE people " +
                        "SET firstname = @firstname, " +
                        "surname = @surname, " +
                        "hometel = @hometel, " +
                        "mobile = @mobile, " +
                        "worktel = @worktel, " +
                        "email = @email, " +
                        "member = @member, " +
                        "manmemo = @manmemo " +
                        "WHERE id = @id");
                save.ExecuteNonQuery(p);
            }
            this.DialogResult = true;
            this.Close();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            SelectCrewBoats d = new SelectCrewBoats(Id);
            if (d.ShowDialog() == true)
                Crewing.ItemsSource = Person.BoatCrewFill(Id);
        }
    }
}
