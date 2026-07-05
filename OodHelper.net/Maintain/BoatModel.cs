using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OodHelper.Data;
using OodHelper.Data.Entities;

namespace OodHelper.Maintain
{
    public class BoatModel : NotifyPropertyChanged
    {
        private readonly IBoatRepository _repository;
        private readonly Boat _boat;

        public BoatModel(int b)
        {
            _repository = App.Services.GetRequiredService<IBoatRepository>();
            //
            // Existing boat: load it. New boat (no row): start from a blank entity, defaulting
            // dinghy to false as the old code did. Bid stays 0 to mark it as not-yet-inserted.
            //
            _boat = _repository.Get(b) ?? new Boat { Dinghy = false };
        }

        public string Keel
        {
            get { return _boat.Keel ?? string.Empty; }
            set { _boat.Keel = value; OnPropertyChanged("Keel"); }
        }

        public bool? Dinghy
        {
            get { return _boat.Dinghy; }
            set { _boat.Dinghy = value; OnPropertyChanged("Dinghy"); }
        }

        public string HullType
        {
            get { return _boat.Hulltype ?? string.Empty; }
            set { _boat.Hulltype = value; OnPropertyChanged("HullType"); }
        }

        public string HandicapStatus
        {
            get { return _boat.HandicapStatus ?? string.Empty; }
            set { _boat.HandicapStatus = value; OnPropertyChanged("HandicapStatus"); }
        }

        public string OpenHandicap
        {
            get { return _boat.OpenHandicap?.ToString() ?? string.Empty; }

            set
            {
                if (value == string.Empty)
                    _boat.OpenHandicap = null;
                else if (int.TryParse(value, out int ohp))
                    _boat.OpenHandicap = ohp;
                OnPropertyChanged("OpenHandicap");
            }
        }

        public string RollingHandicap
        {
            get { return _boat.RollingHandicap?.ToString() ?? string.Empty; }

            set
            {
                if (value == string.Empty)
                    _boat.RollingHandicap = null;
                else if (int.TryParse(value, out int ohp))
                    _boat.RollingHandicap = ohp;
                OnPropertyChanged("RollingHandicap");
            }
        }

        public string SmallCatHandicapRating
        {
            get { return _boat.SmallCatHandicapRating?.ToString() ?? string.Empty; }

            set
            {
                if (value == string.Empty)
                    _boat.SmallCatHandicapRating = null;
                else if (decimal.TryParse(value, out decimal schr) && schr < 10 && schr >= 0)
                    _boat.SmallCatHandicapRating = schr;
                OnPropertyChanged("SmallCatHandicapRating");
            }
        }

        public string EnginePropeller
        {
            get { return _boat.EnginePropeller ?? string.Empty; }
            set { _boat.EnginePropeller = value; OnPropertyChanged("EnginePropeller"); }
        }

        public int? Bid
        {
            get { return _boat.Bid == 0 ? (int?)null : _boat.Bid; }
        }

        public string BoatName
        {
            get { return _boat.Boatname ?? string.Empty; }
            set { _boat.Boatname = value; OnPropertyChanged("BoatName"); }
        }

        public string BoatClass
        {
            get { return _boat.Boatclass ?? string.Empty; }
            set { _boat.Boatclass = value; OnPropertyChanged("BoatClass"); }
        }

        public string SailNumber
        {
            get { return _boat.Sailno ?? string.Empty; }
            set { _boat.Sailno = value; OnPropertyChanged("SailNumber"); }
        }

        public string Deviations
        {
            get { return _boat.Deviations ?? string.Empty; }
            set { _boat.Deviations = value; OnPropertyChanged("Deviations"); }
        }

        public string BoatMemo
        {
            get { return _boat.Boatmemo ?? string.Empty; }
            set { _boat.Boatmemo = value; OnPropertyChanged("BoatMemo"); }
        }

        public string CommitChanges()
        {
            var errors = new StringBuilder(string.Empty);
            if (string.IsNullOrEmpty(BoatName) || BoatName.Trim() == string.Empty)
                errors.Append("Boat name required\n");
            ValidateRequiredInteger(OpenHandicap, "Open handicap", errors);
            ValidateRequiredInteger(RollingHandicap, "Rolling handicap", errors);

            if (errors.ToString() == string.Empty)
            {
                _repository.Save(_boat);
                return string.Empty;
            }

            return errors.ToString();
        }

        private static void ValidateRequiredInteger(string value, string valueName, StringBuilder errors)
        {
            if (string.IsNullOrWhiteSpace(value))
                errors.Append($"{valueName} must be entered");
            else if (!int.TryParse(value, out _))
                errors.Append($"{valueName} must be integer value");
        }
    }
}
