using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OodHelper.Results.Model
{
    class RaceResults
    {
        readonly int _rid;

        public string Course { get; set; }
        public string WindSpeed { get; set; }
        public string WindDirection { get; set; }
        public int? Laps { get; set; }
        public Calendar.RaceTypes RaceType { get; set; }
        public string RaceClass { get; set; }
        public string RaceName { get; set; }
        public DateTime? StartDate { get; set; }
        public string Ood { get; set; }
        public string Handicap { get; set; }
        public TimeSpan? TimeLimit { get; set; }
        public TimeSpan Extension { get; set; }

        public RaceResults(int Rid)
        {
            _rid = Rid;
        }
    }
}
