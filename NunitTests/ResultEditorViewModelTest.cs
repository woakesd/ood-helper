using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NunitTests
{
    [TestFixture]
    public class ResultEditorViewModelTest
    {
        private OodHelper.Results.Model.CalendarEvent CalendarEvent;
        private OodHelper.Results.ViewModel.ResultsEditorViewModel EditorViewModel;

        /// <summary>
        /// Need to set up different types of races.
        /// </summary>
        [TestFixtureSetUp]
        public void SetUpTest()
        {
            OodHelper.Results.Model.Race _r = new OodHelper.Results.Model.Race(-1);
            CalendarEvent = new OodHelper.Results.Model.CalendarEvent(new Hashtable());

            EditorViewModel = new OodHelper.Results.ViewModel.ResultsEditorViewModel(_r);
        }

        [TestFixtureTearDown]
        public void TearDownTest()
        {
        }

        [Test]
        public void SetStartTime()
        {
            Assert.False(true);
        }
    }
}
