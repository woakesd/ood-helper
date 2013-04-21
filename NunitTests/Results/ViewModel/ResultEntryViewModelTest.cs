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
            Assert.AreEqual(DateTime.Today, _entryVM.StartDate, "Start date set to today");
        }

        [Test]
        public void StartTimeNotNullTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            DateTime _now = DateTime.Now.AddDays(-1);

            _entry.SetupProperty(d => d.start_date, _now);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test1 = "15:45:12";
            _entryVM.StartTime = _test1;

            Expect(_entryVM.StartTime == _test1, "Start time accepted, overriding entry start");
            Assert.AreEqual(_now.Date, _entryVM.StartDate, "Start date still as for now");
        }

        [Test]
        public void StartTimeEventStartSetTest()
        {
            DateTime _now = DateTime.Now.AddDays(-12);

            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.start_date, _now);
            _event.SetupProperty(d => d.racetype, CalendarEvent.RaceTypes.AverageLap);

            Mock<IEntry> _entry = new Mock<IEntry>();
            _entry.SetupProperty(d => d.start_date);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, _event.Object);

            string _test1 = "15:22:39";
            _entryVM.StartTime = _test1;

            Expect(_entryVM.StartTime == _test1, "Start time accepted, overriding entry start");
            Assert.AreEqual(_now.Date, _entryVM.StartDate, "Start date still as for now");
        }

        [Test]
        public void FinishTimeNullTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();

            _entry.SetupProperty(d => d.finish_date);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test1 = "14:35:32";
            _entryVM.FinishTime = _test1;

            Expect(_entryVM.FinishTime == _test1, "Finish time accepted, overriding null finish");
            Assert.AreEqual(DateTime.Today, _entryVM.FinishDate, "Start date set today");
        }

        [Test]
        public void FinishTimeNotNullTest()
        {
            Random _rnd = new Random();
            Mock<IEntry> _entry = new Mock<IEntry>();
            DateTime _test = DateTime.Now.AddDays(_rnd.Next(-30, 0));

            _entry.SetupProperty(d => d.finish_date, _test);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test1 = "14:35:32";
            _entryVM.FinishTime = _test1;

            Expect(_entryVM.FinishTime == _test1, "Finish time accepted, overriding null finish");
            Assert.AreEqual(_test.Date, _entryVM.FinishDate, "Start date set today");
        }

        [Test]
        public void FinishTimeEventStartTimeTest()
        {
            Random _rnd = new Random();
            DateTime _test = DateTime.Now.AddDays(_rnd.Next(-30, 0));

            Mock<IEntry> _entry = new Mock<IEntry>();
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();

            _event.SetupProperty(d => d.start_date, _test);
            _entry.SetupProperty(d => d.finish_date);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, _event.Object);

            string _test1 = "14:35:32";
            _entryVM.FinishTime = _test1;

            Expect(_entryVM.FinishTime == _test1, "Finish time accepted, overriding null finish");
            Assert.AreEqual(_test.Date, _entryVM.FinishDate, "Finish date set to start date");
        }

        [Test]
        public void ReadBidTest()
        {
            Random _rnd = new Random();
            int _test = _rnd.Next(9999);
            Mock<IEntry> _entry = new Mock<IEntry>();

            _entry.SetupProperty(d => d.bid, _test);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(_test, _entryVM.Bid, "Read Bid");
        }

        [Test]
        public void ReadBoatNameTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            string _test = "Rothi";

            _entry.SetupProperty(d => d.boatname, _test);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(_test, _entryVM.BoatName, "Read Boatname");
        }

        [Test]
        public void ReadBoatClassTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            string _test = "Laser";

            _entry.SetupProperty(d => d.boatclass, _test);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(_test, _entryVM.BoatClass, "Read BoatClass");
        }

        [Test]
        public void ReadSailnoTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            string _test = "182232";

            _entry.SetupProperty(d => d.sailno, _test);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(_test, _entryVM.SailNo, "Read SailNo");
        }

        [Test]
        [Ignore]
        public void ReadFinishCodeTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            string _test = "OOD";

            _entry.SetupProperty(d => d.finish_code, _test);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(_test, _entryVM.FinishCode, "Read FinishCode");

            _entryVM.FinishCode = "RET";
            Assert.AreEqual("RET", _entryVM.FinishCode, "Read FinishCode change to RET");

            _entryVM.FinishCode = string.Empty;
            Assert.AreEqual(string.Empty, _entryVM.FinishCode, "Read FinishCode change to RET");

            _entryVM.FinishTime = "RET";
            Assert.AreEqual("RET", _entryVM.FinishCode, "Read FinishCode change to RET");
        }
    }
}
