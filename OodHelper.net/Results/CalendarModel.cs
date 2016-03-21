using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.Results
{
    public class CalendarModel
    {
        public enum RaceTypes { Undefined, FixedLength, AverageLap, HybridOld, Hybrid, TimeGate, SternChase, }

        public int Rid { get; set; }

        public string Event { get; set; }

        public RaceTypes RaceType { get; set; }

        public string EventClass { get; set; }

        public string Course { get; set; }

        public string WindSpeed { get; set; }

        public string WindDirection { get; set; }

        public int Laps { get; set; }

        public string Ood { get; set; }

        public DateTime StartDate { get; set; }

        public string Handicap { get; set; }

        public int Extension { get; set; }

        public DateTime TimeLimitFixed { get; set; }

        public int TimeLimitDelta { get; set; }
    }
}
