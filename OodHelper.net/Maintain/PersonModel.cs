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

        private WebService.People Person { get; set; }

        public PersonModel(int id)
        {
            WebService.People[] _people = WebService.People.GetPerson(id);
            if (_people != null && _people.Length > 0)
                Person = _people[0];
            else
                Person = new WebService.People();
        }

        public string FirstName
        {
            get
            {
                return Person.firstname;
            }

            set
            {
                Person.firstname = value;
                OnPropertyChanged("FirstName");
            }
        }

        public string Surname
        {
            get
            {
                return Person.surname;
            }

            set
            {
                Person.surname = value;
                OnPropertyChanged("Surname");
            }
        }

        public string Address1
        {
            get
            {
                return Person.address1;
            }

            set
            {
                Person.address1 = value;
                OnPropertyChanged("Address1");
            }
        }

        public string Address2
        {
            get
            {
                return Person.address2;
            }

            set
            {
                Person.address2 = value; OnPropertyChanged("Address2");
            }
        }

        public string Address3
        {
            get
            {
                return Person.address3;
            }

            set
            {
                Person.address3 = value; OnPropertyChanged("Address3");
            }
        }

        public string Address4
        {
            get
            {
                return Person.address4;
            }

            set
            {
                Person.address4 = value; OnPropertyChanged("Address4");
            }
        }

        public string Postcode
        {
            get
            {
                return Person.postcode;
            }

            set
            {
                Person.postcode = value; OnPropertyChanged("Postcode");
            }
        }

        public string HomePhone
        {
            get
            {
                return Person.hometel;
            }

            set
            {
                Person.hometel = value; OnPropertyChanged("HomePhone");
            }
        }

        public string MobilePhone
        {
            get
            {
                return Person.mobile;
            }

            set
            {
                Person.mobile = value; OnPropertyChanged("MobilePhone");
            }
        }

        public string WorkPhone
        {
            get
            {
                return Person.worktel;
            }

            set
            {
                Person.worktel = value; OnPropertyChanged("WorkPhone");
            }
        }

        public string Email
        {
            get
            {
                return Person.email;
            }

            set
            {
                Person.email = value; OnPropertyChanged("Email");
            }
        }

        public string Club
        {
            get
            {
                return Person.club;
            }

            set
            {
                Person.club = value; OnPropertyChanged("Club");
            }
        }

        public string Membership
        {
            get
            {
                return Person.member;
            }

            set
            {
                Person.member = value; OnPropertyChanged("Membership");
            }
        }

        public string Notes
        {
            get
            {
                return Person.manmemo;
            }

            set
            {
                Person.manmemo = value; OnPropertyChanged("Notes");
            }
        }


        public bool? Paid
        {
            get
            {
                return Person.cp;
            }

            set
            {
                Person.cp = value;
                OnPropertyChanged("Paid");
            }
        }

        public bool? PaperNewsletter
        {
            get
            {
                return Person.papernewsletter;
            }

            set
            {
                Person.papernewsletter = value;
                OnPropertyChanged("PaperNewsletter");
            }
        }

        public bool? HandbookExclude
        {
            get
            {
                return Person.handbookexclude;
            }

            set
            {
                Person.handbookexclude = value;
                OnPropertyChanged("HandbookExclude");
            }
        }

        public int Id
        {
            get { return Person.id; }
            private set { Person.id = value; OnPropertyChanged("Id"); }
        }

        public int? MainId
        {
            get { return Person.sid; }
            private set { Person.sid = value; OnPropertyChanged("MainId"); }
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
            if (Surname.Trim() == string.Empty)
                errors.Append("Surname required\n");

            if (errors.ToString() == string.Empty)
            {
                if (Id != 0)
                    Person.UpdatePeople();
                //Db save;
                //if (Id == 0)
                //{
                //    save = new Db("INSERT INTO people " +
                //        "(main_id, firstname, surname, address1, address2, address3, address4, postcode, " +
                //        "hometel, mobile, worktel, email, club, member, manmemo, cp, papernewsletter, handbookexclude) " +
                //        "VALUES (@main_id, @firstname, @surname, @address1, @address2, @address3, @address4, @postcode, " +
                //        "@hometel, @mobile, @worktel, @email, @club, @member, @manmemo, @cp, @papernewsletter, @handbookexclude )");
                //    Id = save.GetNextIdentity("people", "id");
                //    if (!MainId.HasValue || MainId.Value == 0)
                //        MainId = Id;
                //}
                //else
                //    save = new Db("UPDATE people " +
                //            "SET firstname = @firstname, " +
                //            "surname = @surname, " +
                //            "address1 = @address1, " +
                //            "address2 = @address2, " +
                //            "address3 = @address3, " +
                //            "address4 = @address4, " +
                //            "postcode = @postcode, " +
                //            "hometel = @hometel, " +
                //            "mobile = @mobile, " +
                //            "worktel = @worktel, " +
                //            "email = @email, " +
                //            "club = @club, " +
                //            "member = @member, " +
                //            "manmemo = @manmemo, " +
                //            "cp = @cp, " +
                //            "papernewsletter = @papernewsletter, " +
                //            "handbookexclude = @handbookexclude " +
                //            "WHERE id = @id");

                //save.ExecuteNonQuery(Values);
                return string.Empty;
            }
            else
                return errors.ToString();
        }
    }
}
