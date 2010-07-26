using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class HandicapDb : Db
    {
        public static string DatabaseName = @"data\handicaps.sdf";
        public static string DatabaseConstr = "Data Source=" + DatabaseName;

        public HandicapDb(string sql)
            : base(DatabaseConstr, sql)
        {
        }

        public static new void CreateDb()
        {
            if (File.Exists(".\\data\\handicaps.sdf"))
            {
                File.Move(".\\data\\handicaps.sdf", ".\\data\\handicaps-" + DateTime.Now.Ticks.ToString() + ".sdf");
            }

            SqlCeEngine ce = new SqlCeEngine(DatabaseConstr);
            ce.CreateDatabase();
            ce.Dispose();

            SqlCeConnection con = new SqlCeConnection(DatabaseConstr);
            try
            {
                con.Open();
                SqlCeCommand cmd = con.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE [portsmouth_numbers] (
  [id] int NOT NULL IDENTITY (1,1)
, [class_name] nvarchar(100) NULL
, [no_of_crew] int NULL
, [rig] nvarchar(1) NULL
, [spinnaker] nvarchar(1) NULL
, [engine] nvarchar(3) NULL
, [keel] nvarchar(1) NULL
, [number] int NULL
, [status] nvarchar(1) NULL
, [notes] ntext NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"ALTER TABLE [portsmouth_numbers] ADD CONSTRAINT [PK_portsmouth_numbers] PRIMARY KEY ([id])";
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }
    }
}
