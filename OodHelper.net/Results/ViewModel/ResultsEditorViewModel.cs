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
        public override string DisplayName { get { return string.Format("{0} - {1}", _result.Calendar.Event, _result.Calendar.EventClass); } }
        public string Handicap { get { return _result.Calendar.Handicap; } }
        public Calendar.RaceTypes RaceType { get { return _result.Calendar.RaceType; } set { _result.Calendar.RaceType = value; } }

        public string StartTime
        {
            get
            {
                if (_result.Calendar.StartDate.HasValue)
                    return _result.Calendar.StartDate.Value.ToString("hh:mm");
                return string.Empty;
            }

            set
            {
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
