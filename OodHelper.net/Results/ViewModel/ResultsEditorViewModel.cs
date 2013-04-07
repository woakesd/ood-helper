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
        public override string DisplayName { get { return _result.Calendar.Event; } }
        public string Handicap { get { return _result.Calendar.Handicap; } }

        public ResultsEditorViewModel(OodHelper.Results.Model.Race Result)
        {
            if (Result == null) throw new ArgumentNullException("Result");

            _result = Result;
        }

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
    }
}
