using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Converters;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Results.Scoring;
using OodHelper.Services;

namespace OodHelper.Results
{
    /// <summary>
    /// View-model for a single race tab inside <see cref="RaceResults"/>. Replaces the logic
    /// that used to live in the <c>ResultsEditor</c> UserControl: it loads the calendar header
    /// and race rows via <see cref="IRaceResultsRepository"/>, exposes the same property surface
    /// the XAML binds, and persists every header/cell edit immediately. The handicap scoring
    /// engines (<see cref="IRaceScore"/>) run synchronously off the UI thread behind the progress
    /// dialog (see <see cref="Calculate"/>).
    /// </summary>
    public partial class ResultsEditorViewModel : ObservableObject
    {
        private readonly int _rid;
        private readonly IRaceResultsRepository _repo;
        private readonly IRaceScoreRepository _scoreRepo;
        private readonly IDialogService _dialogs;

        private DateTime _limitDate = DateTime.Now;
        private CalendarModel.RaceTypes _raceType;
        private string _eventname = "";
        private int _extension;
        private string _course = "";
        private string _handicap = "";
        private string _windDirection = "";
        private string _windSpeed = "";
        private int? _laps;
        private int? _timeLimitDelta;
        private DateTime? _timeLimitFixed;

        public ResultsEditorViewModel(int rid, IRaceResultsRepository repo,
            IRaceScoreRepository scoreRepo, IDialogService dialogs)
        {
            _rid = rid;
            _repo = repo;
            _scoreRepo = scoreRepo;
            _dialogs = dialogs;
            PrintIncludeCopies = 1;
        }

        public int Rid => _rid;

        public IRaceScore? Scorer { get; private set; }

        // -- Simple notifying state (no DB side-effects) ------------------------------------

        private DateTime _startDate;
        public DateTime StartDate
        {
            get { return _startDate; }
            private set { SetProperty(ref _startDate, value); }
        }

        private string _raceTitle = "";
        public string RaceTitle
        {
            get { return _raceTitle; }
            private set { SetProperty(ref _raceTitle, value); }
        }

        private string _sct = "";
        public string Sct
        {
            get { return _sct; }
            private set { SetProperty(ref _sct, value); }
        }

        public string RaceName { get; private set; } = "";
        public string RaceClass { get; private set; } = "";
        public string Ood { get; private set; } = "";

        private bool _calculateEnabled;
        public bool CalculateEnabled
        {
            get { return _calculateEnabled; }
            set { SetProperty(ref _calculateEnabled, value); }
        }

        private bool _refreshHandicapsEnabled;
        public bool RefreshHandicapsEnabled
        {
            get { return _refreshHandicapsEnabled; }
            set { SetProperty(ref _refreshHandicapsEnabled, value); }
        }

        private ObservableCollection<ResultRowViewModel> _rows = new();
        public ObservableCollection<ResultRowViewModel> Rows
        {
            get { return _rows; }
            private set { SetProperty(ref _rows, value); }
        }

        public string Handicap => _handicap;

        public string RaceStart => StartTime.ToString("hh\\:mm");

        // -- Header fields that persist immediately -----------------------------------------

        public string Course
        {
            get { return _course; }
            set
            {
                _course = value;
                _repo.UpdateCourse(_rid, _course);
                OnPropertyChanged();
            }
        }

        public string WindSpeed
        {
            get { return _windSpeed; }
            set
            {
                _windSpeed = value;
                _repo.UpdateWindSpeed(_rid, _windSpeed);
                OnPropertyChanged();
            }
        }

        public string WindDirection
        {
            get { return _windDirection; }
            set
            {
                _windDirection = value;
                _repo.UpdateWindDirection(_rid, _windDirection);
                OnPropertyChanged();
            }
        }

        public string Laps
        {
            get { return _laps?.ToString() ?? string.Empty; }
            set
            {
                _laps = ValueParsers.ReadInt(value);
                _repo.UpdateLaps(_rid, _laps);
                OnPropertyChanged();
            }
        }

        public CalendarModel.RaceTypes RaceType
        {
            get { return _raceType; }
            set
            {
                _raceType = value;
                _repo.UpdateRaceType(_rid, _raceType.ToString());

                OnPropertyChanged(nameof(RaceType));
                OnPropertyChanged(nameof(StartReadOnly));
                OnPropertyChanged(nameof(StartTimeVisible));
                OnPropertyChanged(nameof(StartDateVisible));
                OnPropertyChanged(nameof(InterimReadOnly));
                OnPropertyChanged(nameof(InterimTimeVisible));
                OnPropertyChanged(nameof(InterimDateVisible));
                OnPropertyChanged(nameof(FinishReadOnly));
                OnPropertyChanged(nameof(FinishTimeVisible));
                OnPropertyChanged(nameof(FinishDateVisible));
                OnPropertyChanged(nameof(LapsEnabled));
                OnPropertyChanged(nameof(LapsReadOnly));
                OnPropertyChanged(nameof(LapsVisible));
                OnPropertyChanged(nameof(PlaceReadOnly));
                OnPropertyChanged(nameof(StdCorrectedVisible));
                OnPropertyChanged(nameof(CorrectedVisible));

                CalculateEnabled = true;

                CreateScorer();
            }
        }

        public TimeSpan StartTime
        {
            get { return StartDate.TimeOfDay; }
            set
            {
                if (value > TimeSpan.FromDays(1.0) || StartDate.Date + value >= StartDate.AddDays(1))
                    MessageBox.Show("You cannot set the start time to this value", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                else if (StartDate.TimeOfDay != value)
                {
                    StartDate = StartDate.Date + value;
                    _repo.UpdateStartDate(_rid, StartDate);
                    Load();
                }
            }
        }

        public TimeSpan? TimeLimit
        {
            get
            {
                if (_timeLimitFixed.HasValue)
                    return _timeLimitFixed.Value.TimeOfDay;
                if (_timeLimitDelta.HasValue)
                    return new TimeSpan(0, 0, _timeLimitDelta.Value);
                return null;
            }
            set
            {
                if (_timeLimitFixed.HasValue)
                {
                    if (value >= TimeSpan.FromDays(1.0) ||
                        _timeLimitFixed.Value.Date + value >= _timeLimitFixed.Value.Date.AddDays(1))
                        MessageBox.Show("You cannot set the start time to this value", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        _timeLimitFixed = _timeLimitFixed.Value.Date + value;
                        _repo.UpdateTimeLimitFixed(_rid, _timeLimitFixed!.Value);
                    }
                }
                else if (_timeLimitDelta.HasValue)
                {
                    if (value >= TimeSpan.FromDays(1.0))
                        MessageBox.Show("You cannot set the start time to this value", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        _timeLimitDelta = (int) value!.Value.TotalSeconds;
                        _repo.UpdateTimeLimitDelta(_rid, _timeLimitDelta.Value);
                    }
                }
            }
        }

        public TimeSpan Extension
        {
            get { return new TimeSpan(0, 0, _extension); }
            set
            {
                _extension = (int) value.TotalSeconds;
                _repo.UpdateExtension(_rid, _extension);
            }
        }

        // -- Per-race-type read-only / visibility (ported verbatim) -------------------------

        public bool LapsEnabled => RaceType != CalendarModel.RaceTypes.AverageLap;

        public bool PlaceReadOnly
        {
            get
            {
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.SternChase:
                        return false;
                }
                return true;
            }
        }

        public bool DisplayDate => StartDate.Date != _limitDate.Date;

        public bool StartReadOnly
        {
            get
            {
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.SternChase:
                    case CalendarModel.RaceTypes.TimeGate:
                        return false;
                }
                return true;
            }
        }

        public Visibility StartTimeVisible => !StartReadOnly ? Visibility.Visible : Visibility.Collapsed;

        public Visibility StartDateVisible =>
            DisplayDate && !StartReadOnly ? Visibility.Visible : Visibility.Collapsed;

        public bool FinishReadOnly
        {
            get
            {
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.SternChase:
                        return true;
                }
                return false;
            }
        }

        public Visibility FinishTimeVisible => !FinishReadOnly ? Visibility.Visible : Visibility.Collapsed;

        public Visibility FinishDateVisible =>
            DisplayDate && !FinishReadOnly ? Visibility.Visible : Visibility.Collapsed;

        public bool InterimReadOnly =>
            RaceType != CalendarModel.RaceTypes.HybridOld && RaceType != CalendarModel.RaceTypes.Hybrid;

        public Visibility InterimTimeVisible => !InterimReadOnly ? Visibility.Visible : Visibility.Collapsed;

        public Visibility InterimDateVisible =>
            DisplayDate && !InterimReadOnly ? Visibility.Visible : Visibility.Collapsed;

        public bool LapsReadOnly
        {
            get
            {
                switch (RaceType)
                {
                    case CalendarModel.RaceTypes.AverageLap:
                    case CalendarModel.RaceTypes.HybridOld:
                    case CalendarModel.RaceTypes.Hybrid:
                        return false;
                    default:
                        return true;
                }
            }
        }

        public Visibility LapsVisible => !LapsReadOnly ? Visibility.Visible : Visibility.Hidden;

        public Visibility StdCorrectedVisible =>
            RaceType != CalendarModel.RaceTypes.SternChase && Handicap == "o"
                ? Visibility.Visible : Visibility.Collapsed;

        public Visibility CorrectedVisible =>
            RaceType != CalendarModel.RaceTypes.SternChase && Handicap == "r"
                ? Visibility.Visible : Visibility.Collapsed;

        public Visibility OpenHandicapVisible => Handicap == "o" ? Visibility.Visible : Visibility.Collapsed;

        public Visibility RollingHandicapVisible => Handicap == "r" ? Visibility.Visible : Visibility.Collapsed;

        // -- Print selection state (consumed by ResultsPrintSelector via the view) ----------

        public bool PrintIncludeAllVisible { get; set; }
        public bool PrintIncludeAll { get; set; }
        public bool PrintInclude { get; set; }
        public int PrintIncludeCopies { get; set; }
        public int PrintIncludeGroup { get; set; }

        // -----------------------------------------------------------------------------------

        public void Load()
        {
            var cal = _repo.GetCalendar(_rid);
            if (cal == null) return;

            StartDate = cal.StartDate ?? DateTime.MinValue;

            _timeLimitFixed = cal.TimeLimitFixed;
            _timeLimitDelta = cal.TimeLimitDelta;
            if (_timeLimitFixed.HasValue)
                _limitDate = _timeLimitFixed.Value;
            else if (_timeLimitDelta.HasValue)
                _limitDate = StartDate.AddSeconds(_timeLimitDelta.Value);

            _extension = cal.Extension ?? 0;

            string method;
            switch (cal.Handicapping)
            {
                case "r": method = "Rolling Handicap"; break;
                case "o": method = "Open Handicap"; break;
                default: method = "No handicapping method"; break;
            }
            RaceTitle = StartDate.ToString("ddd dd MMM yyyy") + " (" + method + ")";

            _eventname = (cal.Event ?? string.Empty).Trim();
            RaceName = _eventname + " - " + (cal.Class ?? string.Empty).Trim();
            RaceClass = (cal.Class ?? string.Empty).Trim();
            Ood = cal.Ood ?? "";
            if (cal.Handicapping != null)
                _handicap = cal.Handicapping;
            Sct = cal.StandardCorrectedTime.HasValue ? Common.HMS(cal.StandardCorrectedTime.Value) : string.Empty;

            if (cal.Racetype != null)
            {
                if (!Enum.TryParse(cal.Racetype, out _raceType))
                    _raceType = CalendarModel.RaceTypes.Undefined;
            }

            CalculateEnabled = _repo.GetCalculateEnabled(_rid);
            RefreshHandicapsEnabled = _repo.GetRefreshHandicapsEnabled(_rid);

            _course = cal.CourseChoice ?? "";
            _windSpeed = cal.WindSpeed ?? "";
            _windDirection = cal.WindDirection ?? "";
            _laps = cal.LapsCompleted;

            CreateScorer();

            var rows = _repo.GetRaceRows(_rid);
            Rows = new ObservableCollection<ResultRowViewModel>(
                rows.Select(r => new ResultRowViewModel(r, StartDate, OnRowEdited)));

            // Refresh every binding (header fields read backing fields set above).
            OnPropertyChanged(string.Empty);
        }

        private void OnRowEdited(Race race)
        {
            _repo.UpdateRaceRow(race);
            CalculateEnabled = true;
        }

        private void CreateScorer()
        {
            Scorer = null;
            switch (RaceType)
            {
                case CalendarModel.RaceTypes.AverageLap:
                case CalendarModel.RaceTypes.FixedLength:
                case CalendarModel.RaceTypes.TimeGate:
                case CalendarModel.RaceTypes.HybridOld:
                case CalendarModel.RaceTypes.Hybrid:
                    switch (Handicap)
                    {
                        case "r":
                            Scorer = new HandicapScorer(_scoreRepo, HandicapMode.Rolling);
                            break;
                        case "o":
                            Scorer = new HandicapScorer(_scoreRepo, HandicapMode.Open);
                            break;
                    }
                    break;
                case CalendarModel.RaceTypes.SternChase:
                    Scorer = new SternChaseScorer(_scoreRepo);
                    break;
            }
        }

        public int CountAutoPopulateData()
        {
            return _repo.CountAutoPopulate(_rid);
        }

        public void DoAutoPopulate()
        {
            _repo.DoAutoPopulate(_rid);
            Load();
        }

        // -- Commands -----------------------------------------------------------------------

        [RelayCommand]
        private async Task Calculate()
        {
            if (Scorer == null) return;

            //
            // Scoring is synchronous and self-contained; run it off the UI thread behind the
            // progress dialog (replaces the old BackgroundWorker + Working ctor), then reload and
            // surface any warnings the engine collected.
            //
            await _dialogs.ShowProgressAsync("Calculating results",
                (progress, ct) => Task.Run(() => Scorer.Calculate(_rid), ct));

            Load();

            if (Scorer.Warnings.Count > 0)
                _dialogs.ShowInformation(string.Join("\n\n", Scorer.Warnings), "Results");
        }

        [RelayCommand]
        private void RefreshRolling()
        {
            _repo.RefreshRollingHandicaps(_rid);
            Load();
        }

        [RelayCommand]
        private void Notes()
        {
            _dialogs.ShowRaceNotes(_rid);
        }
    }
}
