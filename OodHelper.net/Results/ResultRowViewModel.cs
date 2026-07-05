using System;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using OodHelper.Converters;
using OodHelper.Data.Entities;

namespace OodHelper.Results
{
    /// <summary>
    /// Row view-model for the race-results grid. Replaces the old <c>ResultModel</c> (which
    /// wrapped a <see cref="System.Data.DataRow"/>) by wrapping a <see cref="Race"/> entity.
    /// Editing a property mutates the entity and persists immediately via the
    /// <paramref name="onEdited"/> callback supplied by the parent editor, preserving the
    /// original "write on every cell change" behaviour.
    /// </summary>
    public sealed class ResultRowViewModel : ObservableObject
    {
        private readonly Race _race;
        private readonly DateTime _startDate;
        private readonly Action<Race> _onEdited;

        public ResultRowViewModel(Race race, DateTime startDate, Action<Race> onEdited)
        {
            _race = race;
            _startDate = startDate;
            _onEdited = onEdited;
        }

        /// <summary>The underlying entity (read-only access for callers that need keys).</summary>
        public Race Entity => _race;

        private void Persist()
        {
            _onEdited?.Invoke(_race);
        }

        public int Rid => _race.Rid;

        public int Bid => _race.Bid;

        public string BoatName => _race.BidNavigation?.Boatname;

        public string BoatClass => _race.BidNavigation?.Boatclass;

        public string SailNo => _race.BidNavigation?.Sailno;

        public DateTime? StartDate
        {
            get { return _race.StartDate?.Date; }

            set
            {
                if (_race.StartDate.HasValue)
                    _race.StartDate = value + _race.StartDate.Value.TimeOfDay;
                else
                    _race.StartDate = value;
                OnPropertyChanged(nameof(StartTime));
                OnPropertyChanged(nameof(StartDate));
                Persist();
            }
        }

        public string StartTime
        {
            get { return _race.StartDate.HasValue ? _race.StartDate.Value.ToString("HH:mm:ss") : string.Empty; }

            set
            {
                TimeSpan resultTime;
                if (TimeSpan.TryParse(value, out resultTime) ||
                    TimeSpan.TryParseExact(value, @"hh\ mm\ ss", null, out resultTime))
                {
                    _race.StartDate = _startDate.Date + resultTime;
                    Persist();
                }
                OnPropertyChanged(nameof(StartTime));
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public string FinishCode
        {
            get { return _race.FinishCode; }
            set
            {
                _race.FinishCode = value;
                OnPropertyChanged(nameof(FinishCode));
                Persist();
            }
        }

        public DateTime? FinishDate
        {
            get { return ReadDate(_race.FinishDate); }

            set
            {
                _race.FinishDate = SetDatePart(value, _race.FinishDate);
                OnPropertyChanged(nameof(FinishTime));
                OnPropertyChanged(nameof(FinishDate));
                Persist();
            }
        }

        public DateTime? InterimDate
        {
            get { return ReadDate(_race.InterimDate); }

            set
            {
                _race.InterimDate = SetDatePart(value, _race.InterimDate);
                OnPropertyChanged(nameof(InterimTime));
                OnPropertyChanged(nameof(InterimDate));
                Persist();
            }
        }

        public string FinishTime
        {
            get { return ReadTime(_race.FinishDate); }

            set
            {
                SetTimePart(value, finish: true);
                OnPropertyChanged(nameof(FinishTime));
                OnPropertyChanged(nameof(FinishDate));
                Persist();
            }
        }

        public string InterimTime
        {
            get { return ReadTime(_race.InterimDate); }

            set
            {
                SetTimePart(value, finish: false);
                OnPropertyChanged(nameof(InterimTime));
                OnPropertyChanged(nameof(InterimDate));
                Persist();
            }
        }

        public bool RestrictedSail
        {
            get { return _race.RestrictedSail == true; }

            set
            {
                if (value)
                {
                    _race.RestrictedSail = true;
                    if (_race.OpenHandicap.HasValue)
                        _race.OpenHandicap = (int) Math.Round(_race.OpenHandicap.Value * Settings.RSCoefficieent);
                    if (_race.RollingHandicap.HasValue)
                        _race.RollingHandicap = (int) Math.Round(_race.RollingHandicap.Value * Settings.RSCoefficieent);
                }
                else
                {
                    _race.RestrictedSail = null;
                    if (_race.OpenHandicap.HasValue)
                        _race.OpenHandicap = (int) Math.Round(_race.OpenHandicap.Value / Settings.RSCoefficieent);
                    if (_race.RollingHandicap.HasValue)
                        _race.RollingHandicap = (int) Math.Round(_race.RollingHandicap.Value / Settings.RSCoefficieent);
                }
                OnPropertyChanged(nameof(OpenHandicap));
                OnPropertyChanged(nameof(RollingHandicap));
                Persist();
            }
        }

        public string OverridePoints
        {
            get { return _race.OverridePoints.HasValue ? _race.OverridePoints.Value.ToString() : string.Empty; }
            set
            {
                _race.OverridePoints = ValueParsers.ReadDouble(value);
                OnPropertyChanged(nameof(OverridePoints));
                Persist();
            }
        }

        public string Laps
        {
            get { return _race.Laps.HasValue ? _race.Laps.Value.ToString() : string.Empty; }
            set
            {
                _race.Laps = ValueParsers.ReadInt(value);
                OnPropertyChanged(nameof(Laps));
                Persist();
            }
        }

        public string Elapsed
        {
            get
            {
                if (!_race.Elapsed.HasValue) return string.Empty;
                var s = new TimeSpan(0, 0, _race.Elapsed.Value);
                return s.ToString(s.Days > 0 ? @"d\ hh\:mm\:ss" : @"hh\:mm\:ss");
            }
        }

        public string Corrected
        {
            get
            {
                if (!_race.Corrected.HasValue) return string.Empty;
                var s = new TimeSpan((long) (_race.Corrected.Value * 10000000));
                return s.ToString(s.Days > 0 ? @"d\ hh\:mm\:ss\.ff" : @"hh\:mm\:ss\.ff");
            }
        }

        public string StandardCorrected
        {
            get
            {
                if (!_race.StandardCorrected.HasValue) return string.Empty;
                var s = new TimeSpan((long) (_race.StandardCorrected.Value * 10000000));
                return s.ToString(s.Days > 0 ? @"d\ hh\:mm\:ss\.ff" : @"hh\:mm\:ss\.ff");
            }
        }

        public string Place
        {
            get { return _race.Place.HasValue ? _race.Place.Value.ToString() : string.Empty; }
            set
            {
                _race.Place = ValueParsers.ReadInt(value);
                OnPropertyChanged(nameof(Place));
                Persist();
            }
        }

        public string Points
        {
            get { return _race.Points.HasValue ? _race.Points.Value.ToString() : string.Empty; }
            set
            {
                _race.Points = ValueParsers.ReadDouble(value);
                OnPropertyChanged(nameof(Points));
                Persist();
            }
        }

        public string OpenHandicap => _race.OpenHandicap.HasValue ? _race.OpenHandicap.Value.ToString() : string.Empty;

        public string RollingHandicap => _race.RollingHandicap.HasValue ? _race.RollingHandicap.Value.ToString() : string.Empty;

        public string AchievedHandicap => _race.AchievedHandicap.HasValue ? _race.AchievedHandicap.Value.ToString() : string.Empty;

        public string NewRollingHandicap => _race.NewRollingHandicap.HasValue ? _race.NewRollingHandicap.Value.ToString() : string.Empty;

        public string HandicapStatus => _race.HandicapStatus;

        public string C => _race.C;

        public string A => _race.A;

        public string PerformanceIndex => _race.PerformanceIndex.HasValue ? _race.PerformanceIndex.Value.ToString() : string.Empty;

        // ---------------------------------------------------------------------------------------

        private static DateTime? ReadDate(DateTime? value)
        {
            return value?.Date;
        }

        private static DateTime? SetDatePart(DateTime? value, DateTime? existing)
        {
            if (existing.HasValue)
                return value + existing.Value.TimeOfDay;
            return value;
        }

        private static string ReadTime(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("HH:mm:ss") : string.Empty;
        }

        private void SetTimePart(string value, bool finish)
        {
            TimeSpan resultTime;
            var finishCode = new Regex("[a-zA-Z]{3,4}");
            DateTime? current = finish ? _race.FinishDate : _race.InterimDate;
            DateTime? newValue;

            if (TimeSpan.TryParseExact(value, @"hh' 'mm' 'ss", null, out resultTime)
                || TimeSpan.TryParseExact(value, @"hhmmss", null, out resultTime)
                || TimeSpan.TryParseExact(value, @"hh':'mm':'ss", null, out resultTime))
            {
                newValue = current.HasValue ? current.Value.Date + resultTime : _startDate.Date + resultTime;
            }
            else if (finishCode.IsMatch(value))
            {
                _race.FinishCode = value;
                return;
            }
            else
            {
                newValue = null;
            }

            if (finish)
                _race.FinishDate = newValue;
            else
                _race.InterimDate = newValue;
        }
    }
}
