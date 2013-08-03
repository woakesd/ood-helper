using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using OodHelper.Results.Model;
using OodHelper.Results.ViewModel;
using OodHelper.Messaging;

namespace NunitTests.Results.ViewModel
{
    [TestFixture]
    public class ResultEditorViewModelTest : AssertionHelper
    {
        /// <summary>
        /// Need to set up different types of races.
        /// </summary>
        [Test]
        public void StartTime()
        {
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.start_date, DateTime.Today);

            Race _race = new OodHelper.Results.Model.Race(_event.Object, null);

            ResultEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultEditorViewModel(_race);
            EditorViewModel.PropertyChanged += EditorViewModel_PropertyChanged;

            EditorViewModel.StartTime = "14:00";
            Expect(EditorViewModel.StartTime, Is.EqualTo("14:00"), "hh:mm time");
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today), "hh:mm still today");

            EditorViewModel.StartTime = "25:00";
            Expect(EditorViewModel.StartTime, Is.Not.EqualTo("25:00"), "25:00 fails");
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today), "25:00 still today");

            EditorViewModel.StartTime = "0:00";
            Expect(EditorViewModel.StartTime, Is.EqualTo("00:00"), "00:00 time");
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today), "00:00 still today");

            EditorViewModel.StartTime = "-1:00";
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today), "-1:00 still today");

            //
            // Test with date set to today.
            // 
            DateTime _testNow = DateTime.Now;
            _event.Object.start_date = _testNow;

            Expect(EditorViewModel.StartTime, Is.EqualTo(_testNow.TimeOfDay.ToString("hh':'mm")), "Now start Time");
            Expect(EditorViewModel.StartDate, Is.EqualTo(_testNow.Date), "Now date");

            //
            // Test in future
            //
            DateTime _testStartDate = DateTime.Today.AddMonths(2);
            _event.Object.start_date = _testStartDate;

            EditorViewModel.StartTime = "14:00";
            Expect(EditorViewModel.StartTime, Is.EqualTo("14:00"), "hh:mm time future");
            Expect(EditorViewModel.StartDate, Is.EqualTo(_testStartDate), "hh:mm date unchanged future");

            EditorViewModel.StartTime = "25:00";
            Expect(EditorViewModel.StartTime, Is.Not.EqualTo("25:00"), "25:00 fails time future");
            Expect(EditorViewModel.StartDate, Is.EqualTo(_testStartDate), "25:00 date unchanged future");

            EditorViewModel.StartTime = "0:00";
            Expect(EditorViewModel.StartTime, Is.EqualTo("00:00"), "00:00 time future");
            Expect(EditorViewModel.StartDate, Is.EqualTo(_testStartDate), "00:00 date unchanged future");

            EditorViewModel.StartTime = "-1:00";
            Expect(EditorViewModel.StartDate, Is.EqualTo(_testStartDate), "-1:00 date unchanged future");

            //
            // Check that property changed for start time and start date have been raised.
            //
            ValidateOnPropertyChangedRaised("StartDate");

            ValidateOnPropertyChangedRaised("StartTime");
        }

        private void ValidateOnPropertyChangedRaised(string PropertyName)
        {
            Expect(_propertyChangeCounts.ContainsKey(PropertyName), Is.True, string.Format("{0} onPropertyChanged not raised", PropertyName));
            Expect(_propertyChangeCounts[PropertyName].GetType() == typeof(int), Is.True, string.Format("{0} onPropertyChanged not int", PropertyName));
            Expect((int)_propertyChangeCounts[PropertyName], Is.GreaterThan(0), string.Format("{0} onPropertyChanged not called", PropertyName));
        }

        private Hashtable _propertyChangeCounts = new Hashtable();

        void EditorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!_propertyChangeCounts.ContainsKey(e.PropertyName))
                _propertyChangeCounts[e.PropertyName] = 0;
            if (_propertyChangeCounts[e.PropertyName].GetType() == typeof(int))
                _propertyChangeCounts[e.PropertyName] = (int)_propertyChangeCounts[e.PropertyName] + 1;
        }

        [Test]
        public void TimeLimitTests()
        {
            DateTime _nowTest = DateTime.Now;

            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.start_date);
            _event.SetupProperty(d => d.time_limit_type);
            _event.SetupProperty(d => d.time_limit_fixed);
            _event.SetupProperty(d => d.time_limit_delta);

            Race _race = new OodHelper.Results.Model.Race(_event.Object, null);

            ResultEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultEditorViewModel(_race);
            EditorViewModel.PropertyChanged += EditorViewModel_PropertyChanged;

            _event.Object.time_limit_type = CalendarEvent.TimeLimitTypes.D;
            EditorViewModel.TimeLimit = "00 15";
            Expect(EditorViewModel.TimeLimit, Is.EqualTo("00:15"), "Delta Time limit 00:15");
            Expect(_event.Object.time_limit_delta, Is.EqualTo(15 * 60), "Event delta time limit 00:15");

            _event.Object.time_limit_type = CalendarEvent.TimeLimitTypes.F;
            EditorViewModel.TimeLimit = "16:00";
            Expect(EditorViewModel.TimeLimit, Is.Empty, "Fixed Time limit 16:00 dates not set");
            _event.Object.time_limit_fixed = _nowTest;
            Expect(EditorViewModel.TimeLimit, Is.EqualTo(_nowTest.TimeOfDay.ToString("hh':'mm")), "Fixed Time limit now time");
            EditorViewModel.TimeLimit = "16:00";
            Expect(EditorViewModel.TimeLimit, Is.EqualTo("16:00"), "Event Fixed time limit 16:00");

            ValidateOnPropertyChangedRaised("TimeLimit");
        }

        [Test]
        public void ExtensionTest()
        {
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.extension);

            Race _race = new OodHelper.Results.Model.Race(_event.Object, null);

            ResultEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultEditorViewModel(_race);
            EditorViewModel.PropertyChanged += EditorViewModel_PropertyChanged;

            _event.Object.extension = 900;
            Expect(EditorViewModel.Extension, Is.EqualTo("00:15"), "Extension read 00:15");

            EditorViewModel.Extension = "0:20";
            Expect(EditorViewModel.Extension, Is.EqualTo("00:20"), "Extension set 00:20");
            Expect(_event.Object.extension, Is.EqualTo(20 * 60), "Extension converted to seconds 00:20");

            ValidateOnPropertyChangedRaised("Extension");
        }

        [Test]
        public void StandardCorrectedTimeTest()
        {
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.standard_corrected_time);

            Race _race = new OodHelper.Results.Model.Race(_event.Object, null);

            _event.Object.standard_corrected_time = 668.0;
            
            ResultEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultEditorViewModel(_race);

            Expect(EditorViewModel.StandardCorrectedTime, Is.EqualTo("00:11:08"), "SCT set to 668s");

            _event.Object.standard_corrected_time = 5438.0;

            Expect(EditorViewModel.StandardCorrectedTime, Is.EqualTo("01:30:38"), "SCT set to 5438s");
        }

        [Test]
        public void WindDirectionTest()
        {
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.wind_direction);

            Race _race = new OodHelper.Results.Model.Race(_event.Object, null);

            ResultEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultEditorViewModel(_race);
            EditorViewModel.PropertyChanged += EditorViewModel_PropertyChanged;

            EditorViewModel.WindDirection = "NE";
            Expect(EditorViewModel.WindDirection, Is.EqualTo("NE"), "Wind direction set to NE");
            Expect(_event.Object.wind_direction, Is.EqualTo("NE"), "Wind direction set to NE");

            ValidateOnPropertyChangedRaised("WindDirection");
        }

        [Test]
        public void WindSpeedTest()
        {
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.wind_speed);

            Race _race = new OodHelper.Results.Model.Race(_event.Object, null);

            ResultEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultEditorViewModel(_race);
            EditorViewModel.PropertyChanged += EditorViewModel_PropertyChanged;

            EditorViewModel.WindSpeed = "20kts";
            Expect(EditorViewModel.WindSpeed, Is.EqualTo("20kts"), "Wind speed set to 20kts");
            Expect(_event.Object.wind_speed, Is.EqualTo("20kts"), "Wind speed set to 20kts");

            ValidateOnPropertyChangedRaised("WindSpeed");
        }

        [Test]
        public void LapsTest()
        {
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.laps);
            _event.SetupProperty(d => d.racetype, CalendarEvent.RaceTypes.FixedLength);

            Race _race = new OodHelper.Results.Model.Race(_event.Object, null);

            ResultEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultEditorViewModel(_race);
            EditorViewModel.PropertyChanged += EditorViewModel_PropertyChanged;

            Assert.AreEqual(CalendarEvent.RaceTypes.FixedLength, EditorViewModel.RaceType, "RaceType isn't FixedLength");

            EditorViewModel.Laps = "X";
            Expect(_propertyChangeCounts.ContainsKey("Laps"), Is.False, string.Format("{0} onPropertyChanged raised when set to X", "Laps"));

            EditorViewModel.Laps = "3";
            Expect(EditorViewModel.Laps, Is.EqualTo("3"), "Laps set to 3");
            Expect(_event.Object.laps, Is.EqualTo(3), "Laps underlying set to 3");

            ValidateOnPropertyChangedRaised("Laps");
        }

        private EventStartChanged _message = null;
        private void EventStartChangeMessage(EventStartChanged Message)
        {
            _message = Message;
        }

        [Test]
        public void StartChangeMessageDateTest()
        {
            int _testRid;
            ResultEditorViewModel EditorViewModel;
            SetUpStartChangeMessageTest(out _testRid, out EditorViewModel);

            //
            // Set test data
            //
            DateTime _test = DateTime.Now;
            _message = null;

            //
            // Set test date
            //
            EditorViewModel.StartDate = _test;

            // Expect message to be now null
            Assert.AreNotEqual(null, _message, "Message not received");
            Assert.AreEqual(_test.Date, _message.Start, "Message set to correct wrong date");
            Assert.AreEqual(_testRid, _message.Rid, "Message set to correct rid");
        }

        [Test]
        public void StartChangeMessageDateTimeGateTest()
        {
            int _testRid;
            ResultEditorViewModel EditorViewModel;
            SetUpStartChangeMessageTest(out _testRid, out EditorViewModel);

            EditorViewModel.RaceType = CalendarEvent.RaceTypes.TimeGate;

            //
            // Set test data
            //
            DateTime _test = DateTime.Now;
            _message = null;

            //
            // Set test date
            //
            EditorViewModel.StartDate = _test;

            // Expect message to be now null
            Assert.AreEqual(null, _message, "Message received");
        }

        [Test]
        public void StartChangeMessageTimeTest()
        {
            int _testRid;
            ResultEditorViewModel EditorViewModel;
            SetUpStartChangeMessageTest(out _testRid, out EditorViewModel);

            //
            // Set test data
            //
            string _test = "12:20";
            _message = null;

            //
            // Set test date
            //
            EditorViewModel.StartTime = _test;

            // Expect message to be now null
            Assert.AreNotEqual(null, _message, "Message not received");
            Assert.AreEqual(_test, _message.Start.Value.ToString("hh:mm"), "Message set to correct wrong time");
            Assert.AreEqual(_testRid, _message.Rid, "Message set to correct rid");
        }

        private void SetUpStartChangeMessageTest(out int _testRid, out ResultEditorViewModel EditorViewModel)
        {
            Random _rnd = new Random();
            //
            //
            //
            _testRid = _rnd.Next(10, 100);
            //
            // Register for message
            //
            Messenger.Default.Register<EventStartChanged>(this, (msg) => EventStartChangeMessage(msg));

            //
            // Set up event mock to store date
            //
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.start_date);
            _event.SetupProperty(d => d.rid, _testRid);
            _event.SetupProperty(d => d.racetype);

            //
            // Create Race
            //
            Race _race = new OodHelper.Results.Model.Race(_event.Object, null);

            //
            // Create ViewModel
            //
            EditorViewModel = new OodHelper.Results.ViewModel.ResultEditorViewModel(_race);
        }
    }
}
