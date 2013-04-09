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
        private OodHelper.Results.Model.Race Race;
        private OodHelper.Results.ViewModel.ResultsEditorViewModel EditorViewModel;

        /// <summary>
        /// Need to set up different types of races.
        /// </summary>
        [TestFixtureSetUp]
        public void SetUpTest()
        {
            Race = new OodHelper.Results.Model.Race();
            Race.Event = null;  //
            Race.Entries = null;

            EditorViewModel = new OodHelper.Results.ViewModel.ResultsEditorViewModel(Race);
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
