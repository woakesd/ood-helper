using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using OodHelper.Results.Model;
using OodHelper.Results.ViewModel;

namespace NunitTests.Results.ViewModel
{
    [TestFixture]
    public class ResultEntryViewModelTest : AssertionHelper
    {
        [Test]
        public void StartTimeNullTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            _entry.SetupProperty(d => d.start_date);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test1 = "11:00:23";
            _entryVM.StartTime = _test1;

            Expect(_entryVM.StartTime == _test1, "Start time accepted, with no racedate set");
            Expect(_entryVM.StartDate == DateTime.Today, "Start date set to today");
        }

        [Test]
        public void StartTimeNotNullTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            DateTime _now = DateTime.Now;

            _entry.SetupProperty(d => d.start_date, _now);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test1 = _now.AddHours(2).TimeOfDay.ToString("hh':'mm':'ss");
            _entryVM.StartTime = _test1;

            Expect(_entryVM.StartTime == _test1, "Start time accepted, overriding entry start");
            Expect(_entryVM.StartDate == _now.Date, "Start date still as for now");
        }
    }
}
