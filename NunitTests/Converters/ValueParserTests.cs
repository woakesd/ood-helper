using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OodHelper.Converters;

namespace NunitTests
{
    [TestFixture]
    public class ValueParserTests : AssertionHelper
    {
        [TestFixtureSetUp]
        public void SetUpTest()
        {
        }

        [TestFixtureTearDown]
        public void TearDownTest()
        {
        }

        [Test]
        public void TimeSpanParse()
        {
            Expect(delegate { return ValueParser.ReadTimeSpan("14:20"); }, Is.EqualTo(new TimeSpan(14, 20, 0)), "Format hh:mm");
            Expect(delegate { return ValueParser.ReadTimeSpan("14 02"); }, Is.EqualTo(new TimeSpan(14, 2, 0)), "Format hh mm");
            Expect(delegate { return ValueParser.ReadTimeSpan("1455"); }, Is.EqualTo(new TimeSpan(14, 55, 0)), "Format hhmm");
            Expect(delegate { return ValueParser.ReadTimeSpan("9:45"); }, Is.EqualTo(new TimeSpan(9, 45, 0)), "Format h:mm");
            Expect(delegate { return ValueParser.ReadTimeSpan("9 45"); }, Is.EqualTo(new TimeSpan(9, 45, 0)), "Format h mm");
            Expect(delegate { return ValueParser.ReadTimeSpan("902"); }, Is.EqualTo(new TimeSpan(9, 2, 0)), "Format hmm");
        }
    }
}
