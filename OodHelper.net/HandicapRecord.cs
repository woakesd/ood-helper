using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper
{
    public class HandicapRecord
    {
        public HandicapRecord()
        {
        }

        public HandicapRecord(Guid Id)
        {
            id = Id;
            using (Db _conn = new Db("SELECT class_name, no_of_crew, rig, spinnaker, engine, keel, number, status, notes FROM portsmouth_numbers WHERE id = @id"))
            {
                Hashtable _para = new Hashtable();
                _para["id"] = id;
                Hashtable _data = _conn.GetHashtable(_para);

                if (_data.Count > 0)
                {
                    class_name = _data["class_name"] as string;
                    no_of_crew = _data["no_of_crew"] as int?;
                    rig = _data["rig"] as string;
                    spinnaker = _data["spinnaker"] as string;
                    engine = _data["engine"] as string;
                    keel = _data["keel"] as string;
                    number = _data["number"] as int?;
                    status = _data["status"] as string;
                    notes = _data["notes"] as string;
                }
            }
        }
        public Guid id { get; set; }

        public string class_name { get; set; }

        public int? no_of_crew { get; set; }

        public string rig { get; set; }

        public string spinnaker { get; set; }

        public string engine { get; set; }

        public string keel { get; set; }

        public int? number { get; set; }

        public string status { get; set; }

        public string notes { get; set; }
    }
}
