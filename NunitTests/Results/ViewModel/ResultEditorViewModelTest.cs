using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using OodHelper.Results.Model;
using OodHelper.Results.ViewModel;

namespace NunitTests.Results.ViewModel
{
    [TestFixture]
    public class ResultEditorTest : AssertionHelper
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

            ResultsEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultsEditorViewModel(_race);
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

            ResultsEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultsEditorViewModel(_race);
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

            ResultsEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultsEditorViewModel(_race);
            EditorViewModel.PropertyChanged += EditorViewModel_PropertyChanged;

            _event.Object.extension = 900;
            Expect(EditorViewModel.Extension, Is.EqualTo("00:15"), "Extension read 00:15");

            EditorViewModel.Extension = "0:20";
            Expect(EditorViewModel.Extension, Is.EqualTo("00:20"), "Extension set 00:20");
            Expect(_event.Object.extension, Is.EqualTo(20 * 60), "Extension converted to seconds 00:20");

            ValidateOnPropertyChangedRaised("Extension");
        }
    }
}
