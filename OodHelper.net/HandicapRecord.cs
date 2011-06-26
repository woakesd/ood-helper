using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq.Mapping;
using System.Text;

namespace OodHelper
{
    [Table(Name = "portsmouth_numbers")]
    public class HandicapRecord
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int id { get; set; }

        [Column(CanBeNull = false)]
        public string class_name { get; set; }

        [Column]
        public int? no_of_crew { get; set; }

        [Column]
        public string rig { get; set; }

        [Column]
        public string spinnaker { get; set; }

        [Column]
        public string engine { get; set; }

        [Column]
        public string keel { get; set; }

        [Column(CanBeNull = false)]
        public int? number { get; set; }

        [Column]
        public string status { get; set; }

        [Column]
        public string notes { get; set; }
    }
}
