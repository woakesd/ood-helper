using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace OodHelper
{
    class FoxproImport
    {
        public FoxproImport()
        {
            OleDbConnection con = new OleDbConnection(@"Provider=vfpoledb;Data Source=C:\Documents and Settings\david\My Documents\peyc;Collating Sequence=general;");
            con.Open();
            OleDbCommand cmd = new OleDbCommand("SELECT * FROM people", con);
            OleDbDataAdapter adp = new OleDbDataAdapter(cmd);
            DataTable d = new DataTable();
            adp.Fill(d);
            Db c = new Db("UPDATE people SET main_id = @sid WHERE id = @id");
            Hashtable p = new Hashtable();
            foreach (DataRow r in d.Rows)
            {
                p["sid"] = r["sid"];
                p["id"] = r["id"];
                c.ExecuteNonQuery(p);
            }
            c.Dispose();
            con.Close();
        }
    }
}
