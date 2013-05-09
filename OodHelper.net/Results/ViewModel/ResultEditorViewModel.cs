using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OodHelper.ViewModel;
using OodHelper.Results.Model;
using OodHelper.Helpers;

namespace OodHelper.Results.ViewModel
{
    public class ResultEditorViewModel : ViewModelBase, IPrintSelectItem
    {
        public ResultEditorViewModel(IRace Result)
        {
            if (Result == null) throw new ArgumentNullException("Result");

            this.Result = Result;
            this.Entries = (from _entry in Result.EventEntries
                            select new ResultEntryViewModel(_entry, Result.Event)).ToList();

            ContextMenuItems = new List<CommandListItem>();
            ContextMenuItems.Add(new CommandListItem()
            {
                Text = "Edit Boat",
                Command =
                    new RelayCommand(execute => this.EditBoat(), canExecute => this.CanEditBoat())
            });
        }

        public readonly IRace Result;

        public IList<ResultEntryViewModel> Entries { get; set; }

        public override string DisplayName { get { return string.Format("{0} - {1}", Result.Event.eventName, Result.Event.eventClass); } }

        RelayCommand _saveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(param => this.Save(), param => this.CanSave);
                }
                return _saveCommand;
            }
        }

        public void Save()
        {
            Result.Save();
        }

        public bool CanSave
        {
            get
            {
                return true;
            }
        }

        RelayCommand _addNotesCommand;
        public RelayCommand AddNotesCommand
        {
            get
            {
                if (_addNotesCommand == null)
                {
                    _addNotesCommand = new RelayCommand(param => this.AddNotes());
                }
                return _addNotesCommand;
            }
        }

        public void AddNotes()
        {
            Result.AddNotes();
        }

        RelayCommand _publishCommand;
        public RelayCommand PublishCommand
        {
            get
            {
                if (_publishCommand == null)
                {
                    _publishCommand = new RelayCommand(param => this.Publish(), param => this.CanPublish);
                }
                return _publishCommand;
            }
        }

        public void Publish()
        {
            Result.Publish();
        }

        public bool CanPublish
        {
            get
            {
                return true;
            }
        }

        RelayCommand _refreshHandicapsCommand;
        public RelayCommand RefreshHandicapsCommand
        {
            get
            {
                if (_refreshHandicapsCommand == null)
                {
                    _refreshHandicapsCommand = new RelayCommand(param => this.RefreshHandicaps(), param => this.CanRefreshHandicaps);
                }
                return _refreshHandicapsCommand;
            }
        }

        public void RefreshHandicaps()
        {
            Result.RefreshHandicaps();
        }

        public bool CanRefreshHandicaps
        {
            get
            {
                return true;
            }
        }

        IList<CommandListItem> _ContextMenuItems;
        public IList<CommandListItem> ContextMenuItems
        {
            get
            {
                return _ContextMenuItems;
            }

            set
            {
                _ContextMenuItems = value;
            }
        }

        public object SelectedEntry { get; set; }

        public CalendarEvent.RaceTypes RaceType
        {
            get
            {
                return Result.Event.racetype;
            }
            
            set
            {
                Result.Event.racetype = value;
                base.OnPropertyChanged("RaceType");
                base.OnPropertyChanged("LapsEnabled");
                base.OnPropertyChanged("StartReadOnly");
                base.OnPropertyChanged("StartDateVisible");
                base.OnPropertyChanged("StartTimeVisible");
                base.OnPropertyChanged("InterimTimeVisible");
                base.OnPropertyChanged("FinishTimeVisible");
                base.OnPropertyChanged("LapsVisible");
                base.OnPropertyChanged("StdCorrectedVisible");
                base.OnPropertyChanged("CorrectedVisible");
            }
        }
        public CalendarEvent.Handicappings Handicapping
        {
            get
            {
                return Result.Event.handicapping;
            }
            set
            {
                Result.Event.handicapping = value;
                base.OnPropertyChanged("RollingHandicapVisible");
                base.OnPropertyChanged("OpenHandicapVisible");
                base.OnPropertyChanged("AchievedHandicapVisible");
                base.OnPropertyChanged("NewRollingHandicapVisible");
            }
        }

        public DateTime StartDate
        {
            get
            {
                if (Result.Event.start_date.HasValue)
                    return Result.Event.start_date.Value.Date;
                return DateTime.Today;
            }

            set
            {
                TimeSpan _tmp;
                if (Result.Event.start_date.HasValue)
                    _tmp = Result.Event.start_date.Value.TimeOfDay;
                else
                    _tmp = TimeSpan.Zero;
                Result.Event.start_date = value + _tmp;
                base.OnPropertyChanged("StartDate");
            }
        }

        public string StartTime
        {
            get
            {
                if (Result.Event.start_date.HasValue)
                    return Result.Event.start_date.Value.ToString("HH:mm");
                return string.Empty;
            }

            set
            {
                TimeSpan? _tmp;
                if ((_tmp = Converters.ValueParser.ReadTimeSpan(value)).HasValue)
                {
                    if (_tmp.Value <= Converters.ValueParser.TwentyFourHours)
                    {
                        Result.Event.start_date = Result.Event.start_date.Value.Date + _tmp.Value;
                        base.OnPropertyChanged("StartTime");
                        base.OnPropertyChanged("StartDate");
                    }
                    base.OnPropertyChanged("StartTime");
                }
            }
        }

        public string TimeLimit
        {
            get
            {
                if (Result.Event.time_limit_type == CalendarEvent.TimeLimitTypes.F && Result.Event.time_limit_fixed.HasValue)
                    return Result.Event.time_limit_fixed.Value.ToString("HH:mm");
                else if (Result.Event.time_limit_type == CalendarEvent.TimeLimitTypes.D && Result.Event.time_limit_delta.HasValue)
                    return new TimeSpan(0, 0, Result.Event.time_limit_delta.Value).ToString("hh\\:mm");
                return string.Empty;
            }

            set
            {
                TimeSpan? _tmp;
                if ((_tmp = Converters.ValueParser.ReadTimeSpan(value)) != null)
                {
                    if (Result.Event.time_limit_type == CalendarEvent.TimeLimitTypes.F)
                    {
                        if (_tmp < Converters.ValueParser.TwentyFourHours)
                        {
                            if (Result.Event.time_limit_fixed.HasValue)
                                Result.Event.time_limit_fixed = Result.Event.time_limit_fixed.Value.Date + _tmp;
                            else if (Result.Event.start_date.HasValue)
                                Result.Event.time_limit_fixed = Result.Event.start_date.Value.Date + _tmp;
                        }
                    }
                    else if (Result.Event.time_limit_type == CalendarEvent.TimeLimitTypes.D)
                        Result.Event.time_limit_delta = (int)_tmp.Value.TotalSeconds;
                    base.OnPropertyChanged("TimeLimit");
                }
            }
        }

        public string Extension
        {
            get
            {
                if (Result.Event.extension.HasValue)
                    return new TimeSpan(0, 0, Result.Event.extension.Value).ToString("hh\\:mm");
                return string.Empty;
            }

            set
            {
                TimeSpan? _tmp;
                if ((_tmp = Converters.ValueParser.ReadTimeSpan(value)) != null)
                {
                    Result.Event.extension = (int)_tmp.Value.TotalSeconds;
                    base.OnPropertyChanged("Extension");
                }
            }
        }
        
        public void BoatUpdated(ResultEntryViewModel EntryVM)
        {
            IEntry _updated = Entry.GetEntries(EntryVM.Rid, EntryVM.Bid, EntryVM.StartDate).FirstOrDefault();
            Entries[Entries.IndexOf(EntryVM)] = EntryVM;
        }

        public bool CanEditBoat()
        {
            return SelectedEntry != null;
        }

        public void EditBoat()
        {
            ResultEntryViewModel _entry = SelectedEntry as ResultEntryViewModel;
            if (_entry != null)
            {
                int bid = _entry.Bid;
                OodHelper.Messaging.Messenger.Default.Send<ResultEntryViewModel>(_entry);
            }
        }

        public bool CanMoveEntry()
        {
            return SelectedEntry != null;
        }

        public void MoveEntry(ResultEditorViewModel To)
        {
            ResultEntryViewModel _entry = SelectedEntry as ResultEntryViewModel;
        }

        public string StandardCorrectedTime
        {
            get
            {
                if (Result.Event.standard_corrected_time.HasValue)
                {
                    long _sct = (long)Math.Round(Result.Event.standard_corrected_time.Value * 1000 * 1000 * 10);
                    TimeSpan _tmp = new TimeSpan(_sct);
                    return _tmp.ToString("hh':'mm':'ss");
                }
                return null;
            }
        }

        public string Course
        {
            get { return Result.Event.course; }
            set { Result.Event.course = value; OnPropertyChanged("Course"); }
        }

        public string WindSpeed
        {
            get { return Result.Event.wind_speed; }
            set { Result.Event.wind_speed = value; OnPropertyChanged("WindSpeed"); }
        }

        public string WindDirection
        {
            get { return Result.Event.wind_direction; }
            set { Result.Event.wind_direction = value; OnPropertyChanged("WindDirection"); }
        }

        public bool LapsEnabled
        {
            get
            {
                return RaceType == CalendarEvent.RaceTypes.FixedLength;
            }
        }

        public string Laps
        {
            get {
                return Result.Event.laps.ToString();
            }

            set
            {
                if (LapsEnabled)
                {
                    int? _tmp;
                    if ((_tmp = Converters.ValueParser.ReadInt(value)) != null)
                    {
                        Result.Event.laps = _tmp.Value;
                        OnPropertyChanged("Laps");
                    }
                }
            }
        }

        public DateTime LimitDate
        {
            get
            {
                if (Result.Event.time_limit_fixed.HasValue)
                    return Result.Event.time_limit_fixed.Value;
                else if (Result.Event.time_limit_delta.HasValue)
                    return StartDate.AddSeconds(Result.Event.time_limit_delta.Value);
                return StartDate;
            }
        }

        public bool DisplayDate
        {
            //
            // If time limit date is not the same day as start date then we show
            // start and finish dates as well as time.
            //
            get
            {
                return StartDate.Date != LimitDate.Date;
            }
        }

        //
        // Start only enterable for Stern Chase and Time Gate races.
        //
        public bool StartReadOnly
        {
            get
            {
                return RaceType != Model.CalendarEvent.RaceTypes.SternChase
                    && RaceType != Model.CalendarEvent.RaceTypes.TimeGate;
            }
        }

        public bool StartDateVisible
        {
            get
            {
                if (DisplayDate && StartTimeVisible)
                    return true;
                return false;
            }
        }

        public bool StartTimeVisible
        {
            get
            {
                return !StartReadOnly;
            }
        }

        public bool InterimDateVisible
        {
            get
            {
                return DisplayDate && InterimTimeVisible;
            }
        }

        public bool InterimTimeVisible
        {
            get
            {
                return RaceType == CalendarEvent.RaceTypes.Hybrid;
            }
        }

        public bool FinishDateVisible
        {
            get
            {
                return DisplayDate && FinishTimeVisible;
            }
        }

        public bool FinishTimeVisible
        {
            get
            {
                return RaceType != CalendarEvent.RaceTypes.SternChase;
            }
        }

        public bool LapsVisible
        {
            get
            {
                return RaceType == CalendarEvent.RaceTypes.AverageLap
                    || RaceType == CalendarEvent.RaceTypes.Hybrid;
            }
        }

        public bool StdCorrectedVisible
        {
            get
            {
                return RaceType != CalendarEvent.RaceTypes.SternChase && Handicapping == CalendarEvent.Handicappings.o;
            }
        }

        public bool CorrectedVisible
        {
            get
            {
                return RaceType != Model.CalendarEvent.RaceTypes.SternChase && Handicapping == CalendarEvent.Handicappings.r;
            }
        }

        public bool OpenHandicapVisible
        {
            get
            {
                return Handicapping == CalendarEvent.Handicappings.o;
            }
        }

        public bool RollingHandicapVisible
        {
            get
            {
                return Handicapping == CalendarEvent.Handicappings.r;
            }
        }

        public bool AchievedHandicapVisible
        {
            get
            {
                return Handicapping == CalendarEvent.Handicappings.r || Handicapping == CalendarEvent.Handicappings.o;
            }
        }

        public bool NewRollingHandicapVisible
        {
            get
            {
                return Handicapping == CalendarEvent.Handicappings.r || Handicapping == CalendarEvent.Handicappings.o;
            }
        }

        #region IPrintSelectItem
        public new void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);
        }

        public bool PrintIncludeAllVisible
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool PrintIncludeAll
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool PrintInclude
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int PrintIncludeCopies
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string PrintIncludeDescription
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int PrintIncludeGroup
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion //IPrintSelectItem
    }
}
