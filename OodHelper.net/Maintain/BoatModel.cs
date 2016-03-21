using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace OodHelper.Maintain
{
    public class BoatModel : NotifyPropertyChanged
    {
        public BoatModel(int b)
        {
            Db get = new Db("SELECT bid, id, boatname, boatclass, sailno, dinghy, " +
                "hulltype, open_handicap, handicap_status, " +
                "rolling_handicap, small_cat_handicap_rating, engine_propeller, keel, deviations, boatmemo " +
                "FROM boats " +
                "WHERE bid = @bid");
            Hashtable p = new Hashtable();
            p["bid"] = b;
            DataTable d = get.GetData(p);
            Values = new Hashtable();
            if (d.Rows.Count > 0)
                foreach (DataColumn c in d.Columns)
                    Values[c.ColumnName] = d.Rows[0][c];
            else
            {
                foreach (DataColumn c in d.Columns)
                    Values[c.ColumnName] = null;
                Dinghy = false;
            }
        }

        public string Keel
        {
            get
            {
                return Values["keel"] as string;
            }

            set
            {
                Values["keel"] = value; OnPropertyChanged("Keel");
            }
        }

        public bool? Dinghy
        {
            get
            {
                return Values["dinghy"] as bool?;
            }

            set
            {
                Values["dinghy"] = value; OnPropertyChanged("Dinghy");
            }
        }

        public string HullType
        {
            get
            {
                return Values["hulltype"] as string;
            }

            set
            {
                Values["hulltype"] = value; OnPropertyChanged("HullType");
            }
        }

        public string HandicapStatus
        {
            get
            {
                return Values["handicap_status"] as string;
            }

            set
            {
                Values["handicap_status"] = value; OnPropertyChanged("HandicapStatus");
            }
        }

        public string OpenHandicap
        {
            get
            {
                if (Values["open_handicap"] != null)
                    return Values["open_handicap"].ToString();
                else
                    return string.Empty;
            }

            set
            {
                if (value == string.Empty)
                    Values["open_handicap"] = null;
                else
                {
                    int ohp;
                    if (Int32.TryParse(value, out ohp))
                        Values["open_handicap"] = ohp;
                }
                OnPropertyChanged("OpenHandicap");
            }
        }

        public string RollingHandicap
        {
            get
            {
                if (Values["rolling_handicap"] != null)
                    return Values["rolling_handicap"].ToString();
                else
                    return string.Empty;
            }

            set
            {
                if (value == string.Empty)
                    Values["rolling_handicap"] = null;
                else
                {
                    int ohp;
                    if (Int32.TryParse(value, out ohp))
                        Values["rolling_handicap"] = ohp;
                }
                OnPropertyChanged("RollingHandicap");
            }
        }

        public string SmallCatHandicapRating
        {
            get
            {
                if (Values["small_cat_handicap_rating"] != null)
                    return Values["small_cat_handicap_rating"].ToString();
                else
                    return string.Empty;
            }

            set
            {
                if (value == string.Empty)
                    Values["small_cat_handicap_rating"] = null;
                else
                {
                    decimal schr;
                    if (Decimal.TryParse(value, out schr) && schr < 10 && schr >= 0)
                    {
                        Values["small_cat_handicap_rating"] = schr;
                    }
                }
                OnPropertyChanged("SmallCatHandicapRating");
            }
        }

        public string EnginePropeller
        {
            get
            {
                return Values["engine_propeller"] as string;
            }

            set
            {
                Values["engine_propeller"] = value;
                OnPropertyChanged("EnginePropeller");
            }
        }

        public int? Bid
        {
            get { return Values["bid"] as int?; }
            set { Values["bid"] = value; }
        }

        public int? Id
        {
            get { return Values["id"] as int?; }
            set { Values["id"] = value; OnPropertyChanged("Id"); OnPropertyChanged("Owner"); }
        }

        public string Owner
        {
            get
            {
                if (Id.HasValue)
                {
                    Db c = new Db("SELECT firstname, surname " +
                        "FROM people " +
                        "WHERE id = @id");
                    Hashtable p = new Hashtable();
                    p["id"] = Id.Value;
                    Hashtable owner = c.GetHashtable(p);
                    if (owner.Count > 0)
                    {
                        return string.Format("{0} {1}", new object[] { owner["firstname"], owner["surname"] });
                    }
                    return string.Empty;
                }
                return string.Empty;
            }
        }

        public string BoatName
        {
            set
            {
                Values["boatname"] = value;
                OnPropertyChanged("BoatName");
            }
            get
            {
                return Values["boatname"] as string;
            }
        }

        public string BoatClass
        {
            set
            {
                Values["boatclass"] = value;
                OnPropertyChanged("BoatClass");
            }
            get
            {
                return Values["boatclass"] as string;
            }
        }

        public string SailNumber
        {
            set
            {
                Values["sailno"] = value;
                OnPropertyChanged("SailNumber");
            }
            get
            {
                return Values["sailno"] as string;
            }
        }

        public string Deviations
        {
            set
            {
                Values["deviations"] = value;
                OnPropertyChanged("Deviations");
            }
            get
            {
                return Values["deviations"] as string;
            }
        }

        public string BoatMemo
        {
            set
            {
                Values["boatmemo"] = value;
                OnPropertyChanged("BoatMemo");
            }
            get
            {
                return Values["boatmemo"] as string;
            }
        }

        public string CommitChanges()
        {
            StringBuilder errors = new StringBuilder(string.Empty);
            if (BoatName.Trim() == string.Empty)
                errors.Append("Boat name required\n");

            if (errors.ToString() == string.Empty)
            {
                Db save;
                if (Bid == null)
                {
                    save = new Db("INSERT INTO boats " +
                            "(id, boatname, boatclass, sailno, dinghy, hulltype, open_handicap, " +
                            "handicap_status, rolling_handicap, small_cat_handicap_rating, " +
                            "engine_propeller, keel, deviations, boatmemo) " +
                            "VALUES (@id, @boatname, @boatclass, @sailno, @dinghy, @hulltype, @open_handicap, " +
                            "@handicap_status, @rolling_handicap, @small_cat_handicap_rating, " +
                            "@engine_propeller, @keel, @deviations, @boatmemo)");
                    Bid = save.GetNextIdentity("boats");
                }
                else
                    save = new Db("UPDATE boats " +
                            "SET id = @id, " +
                            "boatname = @boatname, " +
                            "boatclass = @boatclass, " +
                            "sailno = @sailno, " +
                            "dinghy = @dinghy, " +
                            "hulltype = @hulltype, " +
                            "open_handicap = @open_handicap, " +
                            "handicap_status = @handicap_status, " +
                            "rolling_handicap = @rolling_handicap, " +
                            "small_cat_handicap_rating = @small_cat_handicap_rating, " +
                            "engine_propeller = @engine_propeller, " +
                            "keel = @keel, " +
                            "deviations = @deviations, " +
                            "boatmemo = @boatmemo " +
                            "WHERE bid = @bid");
                save.ExecuteNonQuery(Values);
                return string.Empty;
            }
            else
                return errors.ToString();
        }
    }
}
