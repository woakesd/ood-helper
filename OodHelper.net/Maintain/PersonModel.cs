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

        private WebService.People PersonRecord { get; set; }

        public PersonModel(WebService.People Person)
        {
            PersonRecord = Person;
        }

        public PersonModel(int id)
        {
            if (id != 0)
            {
                PersonRecord = WebService.People.GetPerson(id);
                if (PersonRecord == null)
                    PersonRecord = new WebService.People();
            }
            else
                PersonRecord = new WebService.People();
        }

        public string FirstName
        {
            get
            {
                return PersonRecord.firstname;
            }

            set
            {
                PersonRecord.firstname = value;
                OnPropertyChanged("FirstName");
            }
        }

        public string Surname
        {
            get
            {
                return PersonRecord.surname;
            }

            set
            {
                PersonRecord.surname = value;
                OnPropertyChanged("Surname");
            }
        }

        public string Address1
        {
            get
            {
                return PersonRecord.address1;
            }

            set
            {
                PersonRecord.address1 = value;
                OnPropertyChanged("Address1");
            }
        }

        public string Address2
        {
            get
            {
                return PersonRecord.address2;
            }

            set
            {
                PersonRecord.address2 = value; OnPropertyChanged("Address2");
            }
        }

        public string Address3
        {
            get
            {
                return PersonRecord.address3;
            }

            set
            {
                PersonRecord.address3 = value; OnPropertyChanged("Address3");
            }
        }

        public string Address4
        {
            get
            {
                return PersonRecord.address4;
            }

            set
            {
                PersonRecord.address4 = value; OnPropertyChanged("Address4");
            }
        }

        public string Postcode
        {
            get
            {
                return PersonRecord.postcode;
            }

            set
            {
                PersonRecord.postcode = value; OnPropertyChanged("Postcode");
            }
        }

        public string HomePhone
        {
            get
            {
                return PersonRecord.hometel;
            }

            set
            {
                PersonRecord.hometel = value; OnPropertyChanged("HomePhone");
            }
        }

        public string MobilePhone
        {
            get
            {
                return PersonRecord.mobile;
            }

            set
            {
                PersonRecord.mobile = value; OnPropertyChanged("MobilePhone");
            }
        }

        public string WorkPhone
        {
            get
            {
                return PersonRecord.worktel;
            }

            set
            {
                PersonRecord.worktel = value; OnPropertyChanged("WorkPhone");
            }
        }

        public string Email
        {
            get
            {
                return PersonRecord.email;
            }

            set
            {
                PersonRecord.email = value; OnPropertyChanged("Email");
            }
        }

        public string Club
        {
            get
            {
                return PersonRecord.club;
            }

            set
            {
                PersonRecord.club = value; OnPropertyChanged("Club");
            }
        }

        public string Membership
        {
            get
            {
                return PersonRecord.member;
            }

            set
            {
                PersonRecord.member = value; OnPropertyChanged("Membership");
            }
        }

        public string Notes
        {
            get
            {
                return PersonRecord.manmemo;
            }

            set
            {
                PersonRecord.manmemo = value; OnPropertyChanged("Notes");
            }
        }


        public bool? Paid
        {
            get
            {
                return PersonRecord.cp;
            }

            set
            {
                PersonRecord.cp = value;
                OnPropertyChanged("Paid");
            }
        }

        public bool? PaperNewsletter
        {
            get
            {
                return PersonRecord.papernewsletter;
            }

            set
            {
                PersonRecord.papernewsletter = value;
                OnPropertyChanged("PaperNewsletter");
            }
        }

        public bool? HandbookExclude
        {
            get
            {
                return PersonRecord.handbookexclude;
            }

            set
            {
                PersonRecord.handbookexclude = value;
                OnPropertyChanged("HandbookExclude");
            }
        }

        public int Id
        {
            get { return PersonRecord.id; }
            private set { PersonRecord.id = value; OnPropertyChanged("Id"); }
        }

        public int? MainId
        {
            get { return PersonRecord.sid; }
            private set { PersonRecord.sid = value; OnPropertyChanged("MainId"); }
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
                if (Id == 0)
                    PersonRecord.InsertPeople();
                else
                    PersonRecord.UpdatePeople();
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
