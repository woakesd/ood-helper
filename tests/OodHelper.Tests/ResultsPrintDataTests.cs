using System;
using System.Data;
using System.Linq;
using OodHelper.Data;
using OodHelper.Results;
using Xunit;

namespace OodHelper.Tests
{
    public class ResultsPrintDataTests
    {
        private static RacePrintRow SampleRow() => new RacePrintRow(
            Boat: "Foo (RS)", Class: "Laser", SailNo: "12345", Hcap: 1100,
            FinishCode: null, FinishDate: new DateTime(2026, 6, 1, 14, 30, 0),
            Elapsed: 3600, Laps: 4, Corrected: 3500.5, Place: 1,
            Points: 1.0, AchievedHandicap: 1080, NewRollingHandicap: 1090,
            Percent: -1.8, C: "C", A: "A", Py: "PY");

        [Fact]
        public void BuildTable_ProducesExpectedColumnsInOrder()
        {
            var dt = ResultsPrintData.BuildTable(new[] { SampleRow() });

            var names = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            Assert.Equal(new[]
            {
                "order", "Boat", "Class", "Sail No", "Hcap", "finish_code", "finish_date",
                "Finish", "Elapsed", "Laps", "Corrected", "Pos", "Pts", "Achp", "nhcp",
                "%", "C", "A", "PY"
            }, names);
        }

        [Fact]
        public void BuildTable_MapsRowValues()
        {
            var dt = ResultsPrintData.BuildTable(new[] { SampleRow() });

            DataRow r = dt.Rows[0];
            Assert.Equal(0, r["order"]);
            Assert.Equal("Foo (RS)", r["Boat"]);
            Assert.Equal("Laser", r["Class"]);
            Assert.Equal("12345", r["Sail No"]);
            Assert.Equal(1100, r["Hcap"]);
            Assert.Equal(new DateTime(2026, 6, 1, 14, 30, 0), r["finish_date"]);
            Assert.Equal(string.Empty, r["Finish"]);
            Assert.Equal(3600, r["Elapsed"]);
            Assert.Equal(4, r["Laps"]);
            Assert.Equal(3500.5, r["Corrected"]);
            Assert.Equal(1, r["Pos"]);
            Assert.Equal(1.0, r["Pts"]);
            Assert.Equal(1080, r["Achp"]);
            Assert.Equal(1090, r["nhcp"]);
            Assert.Equal(-1.8, r["%"]);
            Assert.Equal("PY", r["PY"]);
        }

        [Fact]
        public void BuildTable_NullableFields_BecomeDbNull()
        {
            var row = new RacePrintRow(
                Boat: "Bar", Class: null, SailNo: null, Hcap: null,
                FinishCode: "DNF", FinishDate: null,
                Elapsed: null, Laps: null, Corrected: null, Place: 999,
                Points: null, AchievedHandicap: null, NewRollingHandicap: null,
                Percent: null, C: null, A: null, Py: null);

            var dt = ResultsPrintData.BuildTable(new[] { row });
            DataRow r = dt.Rows[0];

            Assert.Equal(DBNull.Value, r["Class"]);
            Assert.Equal(DBNull.Value, r["Hcap"]);
            Assert.Equal("DNF", r["finish_code"]);
            Assert.Equal(DBNull.Value, r["finish_date"]);
            Assert.Equal(DBNull.Value, r["Elapsed"]);
            Assert.Equal(DBNull.Value, r["Corrected"]);
            Assert.Equal(999, r["Pos"]);
            Assert.Equal(DBNull.Value, r["%"]);
        }

        [Fact]
        public void BuildTable_PreservesRowOrder()
        {
            var first = SampleRow() with { Boat = "First", Place = 1 };
            var second = SampleRow() with { Boat = "Second", Place = 2 };

            var dt = ResultsPrintData.BuildTable(new[] { first, second });

            Assert.Equal("First", dt.Rows[0]["Boat"]);
            Assert.Equal("Second", dt.Rows[1]["Boat"]);
        }
    }
}
