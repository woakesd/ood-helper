using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace OodHelper.Maintain
{
    class PersonModel : NotifyPropertyChanged
    {
        public PersonModel(int id, int main_id) : this(id)
        {
            MainId = main_id;
        }

        public PersonModel(int id)
        {
            Db get = new Db("SELECT id, main_id, firstname, surname, address1, address2, address3, address4, " +
                "postcode, hometel, mobile, worktel, email, club, member, manmemo, cp, papernewsletter, handbookexclude " +
                "FROM people " +
                "WHERE id = @id");
            Hashtable p = new Hashtable();
            p["id"] = id;
            DataTable d = get.GetData(p);

            Values = new Hashtable();
            if (d.Rows.Count > 0)
                foreach (DataColumn c in d.Columns)
                    Values[c.ColumnName] = d.Rows[0][c];
            else
            {
                foreach (DataColumn c in d.Columns)
                    Values[c.ColumnName] = null;
            }
        }

        public string FirstName
        {
            get
            {
                return Values["firstname"] as string;
            }

            set
            {
                Values["firstname"] = value; OnPropertyChanged("FirstName");
            }
        }

        public string Surname
        {
            get
            {
                return Values["surname"] as string;
            }

            set
            {
                Values["surname"] = value; OnPropertyChanged("Surname");
            }
        }

        public string Address1
        {
            get
            {
                return Values["address1"] as string;
            }

            set
            {
                Values["address1"] = value; OnPropertyChanged("Address1");
            }
        }

        public string Address2
        {
            get
            {
                return Values["address2"] as string;
            }

            set
            {
                Values["address2"] = value; OnPropertyChanged("Address2");
            }
        }

        public string Address3
        {
            get
            {
                return Values["address3"] as string;
            }

            set
            {
                Values["address3"] = value; OnPropertyChanged("Address3");
            }
        }

        public string Address4
        {
            get
            {
                return Values["address4"] as string;
            }

            set
            {
                Values["address4"] = value; OnPropertyChanged("Address4");
            }
        }

        public string Postcode
        {
            get
            {
                return Values["postcode"] as string;
            }

            set
            {
                Values["postcode"] = value; OnPropertyChanged("Postcode");
            }
        }

        public string HomePhone
        {
            get
            {
                return Values["hometel"] as string;
            }

            set
            {
                Values["hometel"] = value; OnPropertyChanged("HomePhone");
            }
        }

        public string MobilePhone
        {
            get
            {
                return Values["mobile"] as string;
            }

            set
            {
                Values["mobile"] = value; OnPropertyChanged("MobilePhone");
            }
        }

        public string WorkPhone
        {
            get
            {
                return Values["worktel"] as string;
            }

            set
            {
                Values["worktel"] = value; OnPropertyChanged("WorkPhone");
            }
        }

        public string Email
        {
            get
            {
                return Values["email"] as string;
            }

            set
            {
                Values["email"] = value; OnPropertyChanged("Email");
            }
        }

        public string Club
        {
            get
            {
                return Values["club"] as string;
            }

            set
            {
                Values["club"] = value; OnPropertyChanged("Club");
            }
        }

        public string Membership
        {
            get
            {
                return Values["member"] as string;
            }

            set
            {
                Values["member"] = value; OnPropertyChanged("Membership");
            }
        }

        public string Notes
        {
            get
            {
                return Values["manmemo"] as string;
            }

            set
            {
                Values["manmemo"] = value; OnPropertyChanged("Notes");
            }
        }


        public bool? Paid
        {
            get
            {
                return Values["cp"] as bool?;
            }

            set
            {
                Values["cp"] = value;
                OnPropertyChanged("Paid");
            }
        }

        public bool? PaperNewsletter
        {
            get
            {
                return Values["papernewsletter"] as bool?;
            }

            set
            {
                Values["papernewsletter"] = value;
                OnPropertyChanged("PaperNewsletter");
            }
        }

        public bool? HandbookExclude
        {
            get
            {
                return Values["handbookexclude"] as bool?;
            }

            set
            {
                Values["handbookexclude"] = value;
                OnPropertyChanged("HandbookExclude");
            }
        }

        public int Id
        {
            get
            {
                if (Values["id"] != null)
                    return (int)Values["id"];
                else 
                    return 0;
            }
            private set { Values["id"] = value; OnPropertyChanged("Id"); }
        }

        public int? MainId
        {
            get { return Values["main_id"] as int?; }
            private set { Values["main_id"] = value; OnPropertyChanged("MainId"); }
        }

        public DataView Crewing
        {
            get
            {
                Hashtable p = new Hashtable();
                p["id"] = Id;
                Db crewing = new Db("SELECT boats.bid, boatname " +
                    "FROM boats INNER JOIN boat_crew " +
                    "ON boats.bid = boat_crew.bid " +
                    "WHERE boat_crew.id = @id");
                DataTable crw = crewing.GetData(p);
                return crw.DefaultView;
            }
        }

        public string CommitChanges()
        {
            StringBuilder errors = new StringBuilder(string.Empty);
            if (Surname == null || Surname.Trim() == string.Empty)
                errors.Append("Surname required\n");

            if (errors.ToString() == string.Empty)
            {
                Db save;
                if (Id == 0)
                {
                    save = new Db("INSERT INTO people " +
                        "(main_id, firstname, surname, address1, address2, address3, address4, postcode, " +
                        "hometel, mobile, worktel, email, club, member, manmemo, cp, papernewsletter, handbookexclude) " +
                        "VALUES (@main_id, @firstname, @surname, @address1, @address2, @address3, @address4, @postcode, " +
                        "@hometel, @mobile, @worktel, @email, @club, @member, @manmemo, @cp, @papernewsletter, @handbookexclude )");
                    Id = save.GetNextIdentity("people", "id");
                    if (!MainId.HasValue || MainId.Value == 0)
                        MainId = Id;
                }
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
                            "manmemo = @manmemo, " +
                            "cp = @cp, " +
                            "papernewsletter = @papernewsletter, " +
                            "handbookexclude = @handbookexclude " +
                            "WHERE id = @id");

                save.ExecuteNonQuery(Values);
                return string.Empty;
            }
            else
                return errors.ToString();
        }
    }
}
