using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OodHelper.Results.Model;

namespace OodHelper.Results
{
    class ResultsViewModel
    {
        readonly OodHelper.Results.Model.RaceResults _results;

        public ResultsViewModel(OodHelper.Results.Model.RaceResults Results)
        {
            if (Results == null) throw new ArgumentNullException("Results");

            _results = Results;
        }
    }
}
