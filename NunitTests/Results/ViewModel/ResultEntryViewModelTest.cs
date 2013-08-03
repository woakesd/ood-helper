using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using OodHelper.Results.Model;
using OodHelper.Results.ViewModel;
using OodHelper.Messaging;

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
            Assert.AreEqual(DateTime.Today, _entryVM.FinishDate, "Finish date set today");
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
            Assert.AreEqual(_test.Date, _entryVM.FinishDate, "Finish date set today");
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
        public void InterimTimeNullTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();

            _entry.SetupProperty(d => d.interim_date);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test1 = "14:35:32";
            _entryVM.InterimTime = _test1;

            Expect(_entryVM.InterimTime == _test1, "Interim time accepted, overriding null finish");
            Assert.AreEqual(DateTime.Today, _entryVM.InterimDate, "Interim date set today");
        }

        [Test]
        public void InterimTimeNotNullTest()
        {
            Random _rnd = new Random();
            Mock<IEntry> _entry = new Mock<IEntry>();
            DateTime _test = DateTime.Now.AddDays(_rnd.Next(-30, 0));

            _entry.SetupProperty(d => d.interim_date, _test);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test1 = "14:35:32";
            _entryVM.InterimTime = _test1;

            Expect(_entryVM.InterimTime == _test1, "Interim time accepted, overriding null finish");
            Assert.AreEqual(_test.Date, _entryVM.InterimDate, "Interim date set today");
        }

        [Test]
        public void InterimTimeEventStartTimeTest()
        {
            Random _rnd = new Random();
            DateTime _test = DateTime.Now.AddDays(_rnd.Next(-30, 0));

            Mock<IEntry> _entry = new Mock<IEntry>();
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();

            _event.SetupProperty(d => d.start_date, _test);
            _entry.SetupProperty(d => d.interim_date);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, _event.Object);

            string _test1 = "14:35:32";
            _entryVM.InterimTime = _test1;

            Expect(_entryVM.InterimTime == _test1, "Interim time accepted, overriding null interim");
            Assert.AreEqual(_test.Date, _entryVM.InterimDate, "Interim date set to start date");
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
        public void FinishCodeTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            string _test = "OOD";

            _entry.SetupProperty(d => d.finish_code, _test);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(_test, _entryVM.FinishCode, "Read FinishCode");

            _entryVM.FinishCode = _test.ToLower();
            Assert.AreEqual(_test, _entryVM.FinishCode, "Cannot set Finishcode to lower");

            _entryVM.FinishCode = "RET";
            Assert.AreEqual("RET", _entryVM.FinishCode, "Read FinishCode change to RET");

            _entryVM.FinishCode = string.Empty;
            Assert.AreEqual(string.Empty, _entryVM.FinishCode, "Read FinishCode change to empty string");

            _entryVM.FinishTime = "RET";
            Assert.AreEqual("RET", _entryVM.FinishCode, "Read FinishCode change to RET");

            _entryVM.FinishCode = "ocs";
            Assert.AreEqual("OCS", _entryVM.FinishCode, "Read FinishCode change to OCS when set to ocs");

        }

        [Test]
        public void FinishDateTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            DateTime _test = DateTime.Now;

            _entry.SetupProperty(d => d.finish_date);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(false, _entryVM.FinishDate.HasValue, "FinishDate has no value");
            
            _entry.Object.finish_date = _test;
            Assert.AreEqual(_test.Date, _entryVM.FinishDate, "FinishDate is test date");
            Assert.AreEqual(_test.TimeOfDay.ToString("hh':'mm':'ss"), _entryVM.FinishTime, "Finish time is as set");

            _test = DateTime.Now.AddDays(-10).AddMinutes(33);
            _entryVM.FinishDate = _test;
            Assert.AreEqual(_test.Date, _entryVM.FinishDate, "FinishDate is set to FinishTime");
            Assert.AreNotEqual(_test.TimeOfDay.ToString("hh':'mm':'ss"), _entryVM.FinishTime, "FinishTime not changed when FinishDate set");
        }

        [Test]
        public void InterimDateTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            DateTime _test = DateTime.Now;

            _entry.SetupProperty(d => d.interim_date);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(false, _entryVM.InterimDate.HasValue, "InterimDate has no value");

            _entry.Object.interim_date = _test;
            Assert.AreEqual(_test.Date, _entryVM.InterimDate, "InterimDate is test date");
            Assert.AreEqual(_test.TimeOfDay.ToString("hh':'mm':'ss"), _entryVM.InterimTime, "InterimTime is as set");

            _test = DateTime.Now.AddDays(-10).AddMinutes(33);
            _entryVM.InterimDate = _test;
            Assert.AreEqual(_test.Date, _entryVM.InterimDate, "InterimDate is set to InterimTime");
            Assert.AreNotEqual(_test.TimeOfDay.ToString("hh':'mm':'ss"), _entryVM.InterimTime, "InterimTime not changed when InterimuDate set");
        }

        [Test]
        public void OverridePointsTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();

            _entry.SetupProperty(d => d.override_points);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(string.Empty, _entryVM.OverridePoints, "Override points not set by default");

            double _test = 4.2;
            _entryVM.OverridePoints = _test.ToString();
            Assert.AreEqual(_test.ToString(), _entryVM.OverridePoints, "Override point set successfully");
            Assert.AreEqual(_test, _entry.Object.override_points, "Underlying override points set");

            _entryVM.OverridePoints = "";
            Assert.AreEqual(false, _entry.Object.override_points.HasValue, "Value of override points cleared");

            string _testString = "Rubbish";
            _entryVM.OverridePoints = _testString;
            Assert.AreNotEqual(_testString, _entryVM.OverridePoints, "Override points not set to string");
        }

        [Test]
        public void ElapsedTest()
        {
            Random _rnt = new Random();
            Mock<IEntry> _entry = new Mock<IEntry>();

            _entry.SetupProperty(d => d.elapsed);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(string.Empty, _entryVM.Elapsed, "Elapsed not set by default");

            _entry.Object.elapsed = 3600;
            Assert.AreEqual("01:00:00", _entryVM.Elapsed, "Elapsed set to 1 hour");

            int _test = _rnt.Next(1, 24 * 3600 - 1);
            _entry.Object.elapsed = _test;
            Assert.AreEqual(new TimeSpan(0, 0, _test).ToString(@"hh\:mm\:ss"), _entryVM.Elapsed, "Elapsed set to random time less than 1 day");

            _test = _rnt.Next(1, 24 * 3600) + 24 * 3600 * 10;
            _entry.Object.elapsed = _test;
            Assert.AreEqual(new TimeSpan(0, 0, _test).ToString(@"d\ hh\:mm\:ss"), _entryVM.Elapsed, "Elapsed set to random time greater than 1 day");
        }

        [Test]
        public void CorrectedTest()
        {
            Random _rnt = new Random();
            Mock<IEntry> _entry = new Mock<IEntry>();

            _entry.SetupProperty(d => d.corrected);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(string.Empty, _entryVM.Corrected, "Corrected not set by default");

            _entry.Object.corrected = 3600.0;
            Assert.AreEqual("01:00:00.00", _entryVM.Corrected, "Corrected set to 1 hour");

            double _test = _rnt.Next(1, 24 * 3600 - 1) + _rnt.NextDouble();
            _entry.Object.corrected = _test;
            Assert.AreEqual(new TimeSpan((long)(_test * 10000000)).ToString(@"hh\:mm\:ss\.ff"), _entryVM.Corrected, "Corrected set to random time less than 1 day");

            _test = _rnt.Next(1, 24 * 3600) + 24 * 3600 * 10 + _rnt.NextDouble();
            _entry.Object.corrected = _test;
            Assert.AreEqual(new TimeSpan((long)(_test * 10000000)).ToString(@"d\ hh\:mm\:ss\.ff"), _entryVM.Corrected, "Corrected set to random time greater than 1 day");
        }

        [Test]
        public void StandardCorrectedTest()
        {
            Random _rnt = new Random();
            Mock<IEntry> _entry = new Mock<IEntry>();

            _entry.SetupProperty(d => d.standard_corrected);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(string.Empty, _entryVM.StandardCorrected, "Corrected not set by default");

            _entry.Object.standard_corrected = 3600.0;
            Assert.AreEqual("01:00:00.00", _entryVM.StandardCorrected, "Corrected set to 1 hour");

            double _test = _rnt.Next(1, 24 * 3600 - 1) + _rnt.NextDouble();
            _entry.Object.standard_corrected = _test;
            Assert.AreEqual(new TimeSpan((long)(_test * 10000000)).ToString(@"hh\:mm\:ss\.ff"), _entryVM.StandardCorrected, "Corrected set to random time less than 1 day");

            _test = _rnt.Next(1, 24 * 3600) + 24 * 3600 * 10 + _rnt.NextDouble();
            _entry.Object.standard_corrected = _test;
            Assert.AreEqual(new TimeSpan((long)(_test * 10000000)).ToString(@"d\ hh\:mm\:ss\.ff"), _entryVM.StandardCorrected, "Corrected set to random time greater than 1 day");
        }

        [Test]
        public void LapsTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();

            _entry.SetupProperty(d => d.laps);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(string.Empty, _entryVM.Laps, "Laps not set by default");

            int _test = 4;
            _entryVM.Laps = _test.ToString();
            Assert.AreEqual(_test.ToString(), _entryVM.Laps, "Laps set successfully");
            Assert.AreEqual(_test, _entry.Object.laps, "Underlying laps set");

            _entryVM.Laps = "";
            Assert.AreEqual(false, _entry.Object.laps.HasValue, "Value of laps cleared");

            string _testString = "Rubbish";
            _entryVM.Laps = _testString;
            Assert.AreNotEqual(_testString, _entryVM.Laps, "Laps not set to string");
        }

        [Test]
        public void PlaceNoEventTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();

            _entry.SetupProperty(d => d.place);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Assert.AreEqual(string.Empty, _entryVM.Place, "Place not set by default");

            int _test = 4;
            _entryVM.Place = _test.ToString();
            Assert.AreNotEqual(_test.ToString(), _entryVM.Place, "Place not changed as racetype is not set");
            Assert.AreNotEqual(_test, _entry.Object.laps, "Underlying place not set");
        }

        [Test]
        public void PlaceWithEventTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();

            _entry.SetupProperty(d => d.place);
            _event.SetupProperty(d => d.racetype, CalendarEvent.RaceTypes.AverageLap);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, _event.Object);

            Assert.AreEqual(string.Empty, _entryVM.Place, "Place not set by default");

            int _test = 4;

            _entryVM.Place = _test.ToString();
            Assert.AreNotEqual(_test.ToString(), _entryVM.Place, "Place not changed as racetype is AverageLap");
            Assert.AreNotEqual(_test, _entry.Object.place, "Underlying place not set");

            _event.Object.racetype = CalendarEvent.RaceTypes.FixedLength;
            _entryVM.Place = _test.ToString();
            Assert.AreNotEqual(_test.ToString(), _entryVM.Place, "Place not changed as racetype is FixedLength");
            Assert.AreNotEqual(_test, _entry.Object.place, "Underlying place not set");

            _event.Object.racetype = CalendarEvent.RaceTypes.Hybrid;
            _entryVM.Place = _test.ToString();
            Assert.AreNotEqual(_test.ToString(), _entryVM.Place, "Place not changed as racetype is Hybrid");
            Assert.AreNotEqual(_test, _entry.Object.place, "Underlying place not set");

            _event.Object.racetype = CalendarEvent.RaceTypes.TimeGate;
            _entryVM.Place = _test.ToString();
            Assert.AreNotEqual(_test.ToString(), _entryVM.Place, "Place not changed as racetype is TimeGate");
            Assert.AreNotEqual(_test, _entry.Object.place, "Underlying place not set");

            _event.Object.racetype = CalendarEvent.RaceTypes.SternChase;
            _entryVM.Place = _test.ToString();
            Assert.AreEqual(_test.ToString(), _entryVM.Place, "Place changed as racetype is SternChase");
            Assert.AreEqual(_test, _entry.Object.place, "Underlying place set");
        }

        [Test]
        public void ReadPointsTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Random _rnd = new Random();
            int _test = _rnd.Next(10);
            _entry.SetupProperty(d => d.points, _test);

            Assert.AreEqual(_test.ToString(), _entryVM.Points, "Points read correctly");
        }

        [Test]
        public void ReadOpenHandicapTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Random _rnd = new Random();
            int _test = _rnd.Next(500, 1700);
            _entry.SetupProperty(d => d.open_handicap, _test);

            Assert.AreEqual(_test, _entryVM.OpenHandicap, "Open Handicap read correctly");
        }

        [Test]
        public void ReadRollingHandicapTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Random _rnd = new Random();
            int _test = _rnd.Next(500, 1700);
            _entry.SetupProperty(d => d.rolling_handicap, _test);

            Assert.AreEqual(_test, _entryVM.RollingHandicap, "Rolling Handicap read correctly");
        }

        [Test]
        public void ReadAchievedHandicapTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Random _rnd = new Random();
            int _test = _rnd.Next(500, 1700);
            _entry.SetupProperty(d => d.achieved_handicap, _test);

            Assert.AreEqual(_test.ToString(), _entryVM.AchievedHandicap, "Achieved Handicap read correctly");
        }

        [Test]
        public void ReadNewRollingHandicapTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Random _rnd = new Random();
            int _test = _rnd.Next(500, 1700);
            _entry.SetupProperty(d => d.new_rolling_handicap, _test);

            Assert.AreEqual(_test.ToString(), _entryVM.NewRollingHandicap, "Open Handicap read correctly");
        }

        [Test]
        public void ReadHandicapStatus()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test = "PY";
            _entry.SetupProperty(d => d.handicap_status, _test);

            Assert.AreEqual(_test, _entryVM.HandicapStatus, "Handicap Status read correctly");
        }

        [Test]
        public void ReadCTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test = "N";
            _entry.SetupProperty(d => d.c, _test);

            Assert.AreEqual(_test, _entryVM.C, "C read correctly");
        }

        [Test]
        public void ReadATest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            string _test = "S";
            _entry.SetupProperty(d => d.a, _test);

            Assert.AreEqual(_test, _entryVM.A, "A read correctly");
        }

        [Test]
        public void SaveTest()
        {
            Mock<IEntry> _entry = new Mock<IEntry>();
            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            _entryVM.SaveChanges();
            
            _entry.Verify(foo => foo.SaveChanges(), Times.Once(), "SaveChanges wasn't called once");
        }

        [Test]
        public void ReceiveStartDateMessageTest()
        {
            Random _rnd = new Random();
            int _testRid = _rnd.Next(10, 100);
            DateTime _test = DateTime.Now;

            Mock<IEntry> _entry = new Mock<IEntry>();
            _entry.SetupProperty(_d => _d.rid, _testRid);
            _entry.SetupProperty(_d => _d.start_date);

            ResultEntryViewModel _entryVM = new ResultEntryViewModel(_entry.Object, null);

            Messenger.Default.Send(new EventStartChanged() { Rid = _testRid, Start = _test });

            Assert.AreEqual(_test.Date, _entryVM.StartDate, "Start Date not set");
            Assert.AreEqual(_test.TimeOfDay.ToString("hh':'mm':'ss"), _entryVM.StartTime, "Start Time not set");
        }
    }
}
