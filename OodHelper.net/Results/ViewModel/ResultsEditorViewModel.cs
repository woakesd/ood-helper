using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OodHelper.ViewModel;
using OodHelper.Results.Model;

namespace OodHelper.Results.ViewModel
{
    public class ResultsEditorViewModel : ViewModelBase, IPrintSelectItem
    {
        readonly OodHelper.Results.Model.Race _result;

        public IList<Entry> Entries { get { return _result.Entries; } }
        public override string DisplayName { get { return string.Format("{0} - {1}", _result.Calendar.eventName, _result.Calendar.eventClass); } }
        public Calendar.RaceTypes RaceType { get { return _result.Calendar.racetype; } set { _result.Calendar.racetype = value; } }
        public Calendar.Handicappings Handicapping { get { return _result.Calendar.handicapping; } set { _result.Calendar.handicapping = value; } }

        public string StartTime
        {
            get
            {
                if (_result.Calendar.start_date.HasValue)
                    return _result.Calendar.start_date.Value.ToString("HH:mm");
                return string.Empty;
            }

            set
            {
                TimeSpan _tmp;
                if (TimeSpan.TryParseExact(value, "hh\\:mm", null, out _tmp) || TimeSpan.TryParseExact(value, "hh\\ mm", null, out _tmp))
                {
                    _result.Calendar.start_date = _result.Calendar.start_date.Value.Date + _tmp;
                }
            }
        }

        public string TimeLimit
        {
            get
            {
                if (_result.Calendar.time_limit_type == Calendar.TimeLimitTypes.F && _result.Calendar.time_limit_fixed.HasValue)
                    return _result.Calendar.time_limit_fixed.Value.ToString("HH:mm");
                else if (_result.Calendar.time_limit_type == Calendar.TimeLimitTypes.D && _result.Calendar.time_limit_delta.HasValue)
                    return new TimeSpan(0, 0, _result.Calendar.time_limit_delta.Value).ToString("hh\\:mm");
                return string.Empty;
            }

            set
            {
                TimeSpan _tmp;
                if (TimeSpan.TryParseExact(value, "hh\\:mm", null, out _tmp) || TimeSpan.TryParseExact(value, "hh\\ mm", null, out _tmp))
                {
                    if (_result.Calendar.time_limit_type == Calendar.TimeLimitTypes.F)
                    {
                        if (_result.Calendar.time_limit_fixed.HasValue)
                            _result.Calendar.time_limit_fixed = _result.Calendar.time_limit_fixed.Value.Date + _tmp;
                        else if (_result.Calendar.start_date.HasValue)
                            _result.Calendar.time_limit_fixed = _result.Calendar.start_date.Value.Date + _tmp;
                    }
                    else if (_result.Calendar.time_limit_type == Calendar.TimeLimitTypes.D)
                        _result.Calendar.time_limit_delta = (int)_tmp.TotalSeconds;
                }
            }
        }

        public string Extension
        {
            get
            {
                if (_result.Calendar.extension.HasValue)
                    return new TimeSpan(0, 0, _result.Calendar.extension.Value).ToString("hh\\:mm");
                return string.Empty;
            }

            set
            {
                TimeSpan _tmp;
                if (TimeSpan.TryParseExact(value, "hh\\:mm", null, out _tmp) || TimeSpan.TryParseExact(value, "hh\\ mm", null, out _tmp))
                {
                    _result.Calendar.extension = (int)_tmp.TotalSeconds;
                }
            }
        }
        
        public ResultsEditorViewModel(OodHelper.Results.Model.Race Result)
        {
            if (Result == null) throw new ArgumentNullException("Result");

            _result = Result;
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
