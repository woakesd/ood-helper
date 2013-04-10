using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Moq;
using OodHelper.Results.Model;
using OodHelper.Results.ViewModel;

namespace NunitTests
{
    [TestFixture]
    public class ResultEditorViewModelTest : AssertionHelper
    {
        private Race Race;
        private ResultsEditorViewModel EditorViewModel;

        /// <summary>
        /// Need to set up different types of races.
        /// </summary>
        [TestFixtureSetUp]
        public void SetUpTest()
        {
        }

        [TestFixtureTearDown]
        public void TearDownTest()
        {
        }

        [Test]
        public void SetStartTime()
        {
            Mock<ICalendarEvent> _event = new Mock<ICalendarEvent>();
            _event.SetupProperty(d => d.start_date, DateTime.Today);

            Race = new OodHelper.Results.Model.Race(_event.Object, null);

            EditorViewModel = new OodHelper.Results.ViewModel.ResultsEditorViewModel(Race);

            EditorViewModel.StartTime = "14:00";
            Expect(EditorViewModel.StartTime, Is.EqualTo("14:00"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "14 00";
            Expect(EditorViewModel.StartTime, Is.EqualTo("14:00"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "1400";
            Expect(EditorViewModel.StartTime, Is.EqualTo("14:00"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "9:02";
            Expect(EditorViewModel.StartTime, Is.EqualTo("09:02"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "9 02";
            Expect(EditorViewModel.StartTime, Is.EqualTo("09:02"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "902";
            Expect(EditorViewModel.StartTime, Is.EqualTo("09:02"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "0902";
            Expect(EditorViewModel.StartTime, Is.EqualTo("09:02"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "25:00";
            Expect(EditorViewModel.StartTime, Is.Not.EqualTo("25:00"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "0:00";
            Expect(EditorViewModel.StartTime, Is.EqualTo("00:00"));
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "-1:00";
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));

            EditorViewModel.StartTime = "-1";
            Expect(EditorViewModel.StartDate, Is.EqualTo(DateTime.Today));
        }
    }
}
