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
        [Test]
        public void TimeSpanNoSecondsParse()
        {
            Expect(ValueParser.ReadTimeSpan("14:20"), Is.EqualTo(new TimeSpan(14, 20, 0)), "Format hh:mm");
            Expect(ValueParser.ReadTimeSpan("14 02"), Is.EqualTo(new TimeSpan(14, 2, 0)), "Format hh mm");
            Expect(ValueParser.ReadTimeSpan("1455"), Is.EqualTo(new TimeSpan(14, 55, 0)), "Format hhmm");
            Expect(ValueParser.ReadTimeSpan("9:45"), Is.EqualTo(new TimeSpan(9, 45, 0)), "Format h:mm");
            Expect(ValueParser.ReadTimeSpan("9 45"), Is.EqualTo(new TimeSpan(9, 45, 0)), "Format h mm");
            Expect(ValueParser.ReadTimeSpan("902"), Is.EqualTo(new TimeSpan(9, 2, 0)), "Format hmm");

            Expect(ValueParser.ReadTimeSpan("24:00"), Is.Null, "24:00 should fail");
            Expect(ValueParser.ReadTimeSpan("23:61"), Is.Null, "23:61 should fail");

            Expect(ValueParser.ReadTimeSpan("XAAS1200"), Is.Null, "Bad input random alphanumeric");
            Expect(ValueParser.ReadTimeSpan("-1"), Is.Null, "Bad input, -1");
        }

        [Test]
        public void TimeSpanWithSecondsParse()
        {
            Expect(ValueParser.ReadTimeSpan("14:20:22"), Is.EqualTo(new TimeSpan(14, 20, 22)), "Format hh:mm:ss");
            Expect(ValueParser.ReadTimeSpan("14 02 22"), Is.EqualTo(new TimeSpan(14, 2, 22)), "Format hh mm ss");
            Expect(ValueParser.ReadTimeSpan("145535"), Is.EqualTo(new TimeSpan(14, 55, 35)), "Format hhmmss");

            Expect(ValueParser.ReadTimeSpan("24:00:00"), Is.Null, "24:00:00 should fail");
            Expect(ValueParser.ReadTimeSpan("23:59:61"), Is.Null, "23:59:61 should fail");
        }

        [Test]
        public void IntParse()
        {
            int testVal = -1;
            Expect(ValueParser.ReadInt(testVal.ToString()), Is.EqualTo(testVal), testVal.ToString());
            testVal = 23;
            Expect(ValueParser.ReadInt(testVal.ToString()), Is.EqualTo(testVal), testVal.ToString());
            testVal = Int32.MaxValue;
            Expect(ValueParser.ReadInt(testVal.ToString()), Is.EqualTo(testVal), testVal.ToString());
            testVal = Int32.MinValue;
            Expect(ValueParser.ReadInt(testVal.ToString()), Is.EqualTo(testVal), testVal.ToString());

            Expect(ValueParser.ReadInt("XSAS"), Is.Null, "Random string");
            Expect(ValueParser.ReadInt("12.32"), Is.Null, "Floating point");
            Expect(ValueParser.ReadInt("12:00"), Is.Null, "Punctuation");
        }

        [Test]
        public void DoubleParse()
        {
            double testVal = -1.9;
            Expect(ValueParser.ReadDouble(testVal.ToString()), Is.EqualTo(testVal), testVal.ToString());
            testVal = 23;
            Expect(ValueParser.ReadDouble(testVal.ToString()), Is.EqualTo(testVal), testVal.ToString());

            testVal = 123.2221;
            Expect(ValueParser.ReadDouble(testVal.ToString()), Is.EqualTo(testVal), testVal.ToString());

            int testInt = -1;
            Expect(ValueParser.ReadDouble(testInt.ToString()), Is.EqualTo((double)testInt), string.Format("Int test {0}", testInt));

            Expect(ValueParser.ReadDouble("XSAS"), Is.Null, "Random string");
            Expect(ValueParser.ReadDouble("12:00"), Is.Null, "Punctuation");
        }
    }
}
