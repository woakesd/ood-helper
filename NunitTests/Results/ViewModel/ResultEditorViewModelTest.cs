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
        public void SetStart()
        {
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.start_date, DateTime.Today);

            Race Race = new OodHelper.Results.Model.Race(_event.Object, null);

            ResultsEditorViewModel EditorViewModel = new OodHelper.Results.ViewModel.ResultsEditorViewModel(Race);

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
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today), "-1:00 time should fail to change date");

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
            Expect(EditorViewModel.StartTime, Is.EqualTo("14:00"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(_testStartDate));

            EditorViewModel.StartTime = "25:00";
            Expect(EditorViewModel.StartTime, Is.Not.EqualTo("25:00"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(_testStartDate));

            EditorViewModel.StartTime = "0:00";
            Expect(EditorViewModel.StartTime, Is.EqualTo("00:00"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(_testStartDate));

            EditorViewModel.StartTime = "-1:00";
            Expect(EditorViewModel.StartDate, Is.EqualTo(_testStartDate));
        }
    }
}
