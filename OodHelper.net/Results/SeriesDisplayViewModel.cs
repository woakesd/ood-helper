using System.Collections.Generic;

namespace OodHelper.Results
{
    /// <summary>
    /// Read-only view-model for one class/division tab of the series results grid. The fixed columns
    /// (boat, class, sail no, entered, position, score) bind to <see cref="Rows"/>; the variable
    /// number of per-race columns (R1..Rn) is generated from <see cref="RaceColumnCount"/> by the
    /// <c>SeriesRaceColumns</c> attached behaviour, each bound to the row's <see cref="SeriesRowViewModel.Cells"/>.
    /// </summary>
    public sealed class SeriesDisplayViewModel
    {
        public SeriesDisplayViewModel(string seriesName, string className, int entries,
            int raceColumnCount, IReadOnlyList<SeriesRowViewModel> rows)
        {
            SeriesName = seriesName;
            ClassName = className;
            Entries = entries;
            RaceColumnCount = raceColumnCount;
            Rows = rows;
        }

        /// <summary>Full title ("&lt;series&gt; - &lt;class&gt;"), shown on the tab body and print page.</summary>
        public string SeriesName { get; }

        /// <summary>The bare class/division, used as the tab header.</summary>
        public string ClassName { get; }

        /// <summary>Number of boats entered in this division.</summary>
        public int Entries { get; }

        /// <summary>Number of races in the series (drives the generated R1..Rn columns).</summary>
        public int RaceColumnCount { get; }

        public IReadOnlyList<SeriesRowViewModel> Rows { get; }
    }

    /// <summary>One boat's standing in a division, plus its per-race scores in date order.</summary>
    public sealed class SeriesRowViewModel
    {
        public int Bid { get; set; }
        public string Boatname { get; set; }
        public string Boatclass { get; set; }
        public string Sailno { get; set; }
        public int Entered { get; set; }
        public int Place { get; set; }
        public double Score { get; set; }

        /// <summary>Per-race entries, date-ascending; index i aligns with the R(i+1) column.</summary>
        public IReadOnlyList<SeriesEntry> Cells { get; set; }
    }
}
