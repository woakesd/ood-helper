using System;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Services;

namespace OodHelper.ViewModels
{
    /// <summary>
    /// Editor for a single race (calendar row). Absorbs the time formatting/parse logic from the old
    /// Maintain\Calendar.cs view-model and the RaceEdit code-behind (time-limit radio recompute,
    /// validation, save). Replaces both.
    /// </summary>
    public partial class RaceEditViewModel : ObservableObject
    {
        private readonly ICalendarRepository _repository;
        private readonly IPortsmouthNumberRepository _pnRepository;
        private readonly IDialogService _dialogs;
        private bool _loading;

        /// <summary>Raised when the dialog should close; argument is the DialogResult.</summary>
        public event Action<bool> CloseRequested;

        public int Rid { get; private set; }

        [ObservableProperty] private string _event;
        [ObservableProperty] private string _className;
        [ObservableProperty] private string _flag;
        [ObservableProperty] private string _course;
        [ObservableProperty] private string _ood;
        [ObservableProperty] private string _venue;
        [ObservableProperty] private string _memo;
        [ObservableProperty] private string _priceCode;
        [ObservableProperty] private string _handicapping;
        [ObservableProperty] private string _racetype;
        [ObservableProperty] private int? _visitors;
        [ObservableProperty] private bool? _raced;
        [ObservableProperty] private bool? _approved;
        [ObservableProperty] private bool? _isRace;

        [ObservableProperty] private DateTime? _startDateDate;
        [ObservableProperty] private string _startDateTime;
        [ObservableProperty] private DateTime? _timeLimitFixedDate;
        [ObservableProperty] private string _timeLimitFixedTime;
        [ObservableProperty] private string _timeLimitDeltaText;
        [ObservableProperty] private string _extensionText;
        [ObservableProperty] private string _sct;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowFixedTimeLimit))]
        [NotifyPropertyChangedFor(nameof(ShowDeltaTimeLimit))]
        [NotifyPropertyChangedFor(nameof(ShowExtension))]
        [NotifyPropertyChangedFor(nameof(IsTimeFixed))]
        [NotifyPropertyChangedFor(nameof(IsTimeDelta))]
        [NotifyPropertyChangedFor(nameof(IsTimeNone))]
        private string _timeLimitType;

        public bool ShowFixedTimeLimit => TimeLimitType == "F";
        public bool ShowDeltaTimeLimit => TimeLimitType == "D";
        public bool ShowExtension => TimeLimitType == "F" || TimeLimitType == "D";

        //
        // Bound to the three time-limit radio buttons. The setters mirror the old *_Checked handlers:
        // switching to Fixed derives the fixed time from the delta, switching to Delta derives the
        // delta from the fixed time.
        //
        public bool IsTimeFixed
        {
            get => TimeLimitType == "F";
            set { if (value) SetTimeLimitType("F"); }
        }

        public bool IsTimeDelta
        {
            get => TimeLimitType == "D";
            set { if (value) SetTimeLimitType("D"); }
        }

        public bool IsTimeNone
        {
            get => string.IsNullOrEmpty(TimeLimitType);
            set { if (value) SetTimeLimitType(string.Empty); }
        }

        public RaceEditViewModel(ICalendarRepository repository, IPortsmouthNumberRepository pnRepository,
            IDialogService dialogs, int rid)
        {
            _repository = repository;
            _pnRepository = pnRepository;
            _dialogs = dialogs;
            Rid = rid;

            _loading = true;
            try
            {
                if (rid != 0)
                {
                    var c = repository.Get(rid);
                    if (c != null) LoadFrom(c);
                }
                else
                {
                    // New-race defaults, matching the old code-behind.
                    IsRace = true;
                    Raced = false;
                    Approved = false;
                    Racetype = string.Empty;
                    TimeLimitType = "F";
                }
            }
            finally
            {
                _loading = false;
            }
        }

        private void LoadFrom(Calendar c)
        {
            Event = c.Event;
            ClassName = c.Class;
            Flag = c.Flag;
            Course = c.Course;
            Ood = c.Ood;
            Venue = c.Venue;
            Memo = c.Memo;
            PriceCode = c.PriceCode;
            Handicapping = c.Handicapping;
            Racetype = c.Racetype;
            Visitors = c.Visitors;
            Raced = c.Raced;
            Approved = c.Approved;
            IsRace = c.IsRace;

            ExtensionText = FormatDuration(c.Extension);
            TimeLimitDeltaText = FormatDuration(c.TimeLimitDelta);
            Sct = FormatSct(c.StandardCorrectedTime);

            if (c.StartDate.HasValue)
            {
                StartDateDate = c.StartDate.Value.Date;
                StartDateTime = c.StartDate.Value.ToString("HH:mm");
            }
            if (c.TimeLimitFixed.HasValue)
            {
                TimeLimitFixedDate = c.TimeLimitFixed.Value.Date;
                TimeLimitFixedTime = c.TimeLimitFixed.Value.ToString("HH:mm");
            }

            TimeLimitType = c.TimeLimitType ?? string.Empty;
        }

        partial void OnStartDateDateChanged(DateTime? value)
        {
            // When editing a fixed limit, keep its date aligned with the start date (old behaviour).
            if (_loading) return;
            if (IsTimeFixed) TimeLimitFixedDate = value;
        }

        partial void OnTimeLimitFixedDateChanged(DateTime? value)
        {
            if (_loading) return;
            if (value.HasValue && !StartDateDate.HasValue) StartDateDate = value;
        }

        private void SetTimeLimitType(string newType)
        {
            if (TimeLimitType == newType) return;

            if (newType == "F")
            {
                var delta = TryParseDuration(TimeLimitDeltaText);
                var start = TryCombine(StartDateDate, StartDateTime);
                if (delta.HasValue && start.HasValue)
                {
                    var f = start.Value + TimeSpan.FromSeconds(delta.Value);
                    TimeLimitFixedDate = f.Date;
                    TimeLimitFixedTime = f.ToString("HH:mm");
                }
            }
            else if (newType == "D")
            {
                var fixedLimit = TryCombine(TimeLimitFixedDate, TimeLimitFixedTime);
                var start = TryCombine(StartDateDate, StartDateTime);
                if (fixedLimit.HasValue && start.HasValue)
                    TimeLimitDeltaText = FormatDuration((int)(fixedLimit.Value - start.Value).TotalSeconds);
            }

            TimeLimitType = newType;
        }

        [RelayCommand]
        private void SelectClass()
        {
            var id = _dialogs.ShowClassPicker();
            if (id == null) return;

            var pn = _pnRepository.Get(id.Value);
            if (pn != null) ClassName = pn.ClassName;
        }

        [RelayCommand]
        private void Save()
        {
            var msg = new StringBuilder();

            DateTime? startDate = null;
            try { startDate = Combine(StartDateDate, StartDateTime); }
            catch (FormatException) { msg.AppendLine("Start time format must be like 12:50"); }

            DateTime? fixedLimit = null;
            try { fixedLimit = Combine(TimeLimitFixedDate, TimeLimitFixedTime); }
            catch (FormatException) { msg.AppendLine("Time limit time format must be like 12:50"); }

            int? delta = null;
            try { delta = ParseDuration(TimeLimitDeltaText); }
            catch (ArgumentException ex) { msg.AppendLine(ex.Message); }

            int? extension = null;
            try { extension = ParseDuration(ExtensionText); }
            catch (ArgumentException ex) { msg.AppendLine(ex.Message); }

            if (IsRace == true)
            {
                if (string.IsNullOrEmpty(Event)) msg.AppendLine("Event name must be provided");
                if (string.IsNullOrEmpty(ClassName)) msg.AppendLine("Class must be provided");
                if (!startDate.HasValue) msg.AppendLine("Start date and time must be provided");
                switch (TimeLimitType)
                {
                    case "F":
                        if (!fixedLimit.HasValue) msg.AppendLine("Fixed time limit date and time must be provided");
                        if (fixedLimit < startDate) msg.AppendLine("Fixed time limit must be after start");
                        if (!extension.HasValue) msg.AppendLine("Extension must be provided");
                        break;
                    case "D":
                        if (!delta.HasValue) msg.AppendLine("Time limit delta must be provided");
                        if (!extension.HasValue) msg.AppendLine("Extension must be provided");
                        break;
                }
            }

            if (msg.Length > 0)
            {
                _dialogs.ShowError(msg.ToString(), "Input not valid");
                return;
            }

            var entity = new Calendar
            {
                Rid = Rid,
                StartDate = startDate,
                Class = ClassName,
                Event = Event,
                PriceCode = PriceCode,
                Course = Course,
                Ood = Ood,
                Venue = Venue,
                Racetype = Racetype,
                Handicapping = Handicapping,
                Visitors = Visitors,
                Flag = Flag,
                Extension = extension,
                Memo = Memo,
                Raced = Raced,
                Approved = Approved,
                IsRace = IsRace
            };

            switch (TimeLimitType)
            {
                case "F":
                    entity.TimeLimitType = "F";
                    entity.TimeLimitDelta = null;
                    entity.TimeLimitFixed = fixedLimit;
                    break;
                case "D":
                    entity.TimeLimitType = "D";
                    entity.TimeLimitDelta = delta;
                    entity.TimeLimitFixed = null;
                    break;
                default:
                    entity.TimeLimitType = null;
                    entity.TimeLimitDelta = null;
                    entity.TimeLimitFixed = null;
                    break;
            }

            Rid = _repository.Save(entity);
            CloseRequested?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }

        //
        // Time helpers ported from the old Maintain\Calendar.cs view-model. Durations are stored as
        // total seconds and shown as "h:mm" (or "d hh:mm" beyond a day); times of day as "HH:mm".
        //
        private static string FormatDuration(int? seconds)
        {
            if (!seconds.HasValue) return string.Empty;
            var d = new TimeSpan(0, 0, seconds.Value);
            return d < TimeSpan.FromDays(1) ? d.ToString("h\\:mm") : d.ToString("d\\ hh\\:mm");
        }

        private static string FormatSct(double? standardCorrectedTime)
        {
            if (!standardCorrectedTime.HasValue) return string.Empty;
            var t = new TimeSpan((long)(standardCorrectedTime.Value * 10000000));
            return t.ToString("hh\\:mm\\:ss\\.ff");
        }

        /// <summary>Parses a duration ("1 02:50" or "2:30") into total seconds; throws on bad input.</summary>
        private static int? ParseDuration(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            try { return (int)TimeSpan.ParseExact(text, "d\\ hh\\:mm", null).TotalSeconds; }
            catch (FormatException) { }
            try { return (int)TimeSpan.ParseExact(text, "h\\:mm", null).TotalSeconds; }
            catch (FormatException) { throw new ArgumentException("Time must be in format '1 02:50' or '2:30'"); }
        }

        private static int? TryParseDuration(string text)
        {
            try { return ParseDuration(text); }
            catch (ArgumentException) { return null; }
        }

        /// <summary>Combines a date and an "h:mm" time string; throws on a bad time.</summary>
        private static DateTime? Combine(DateTime? date, string time)
        {
            if (!date.HasValue) return null;
            if (string.IsNullOrEmpty(time)) return date.Value.Date;
            return date.Value.Date + TimeSpan.ParseExact(time, "h\\:mm", null);
        }

        private static DateTime? TryCombine(DateTime? date, string time)
        {
            try { return Combine(date, time); }
            catch (FormatException) { return null; }
        }
    }
}
