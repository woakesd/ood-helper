using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;

namespace OodHelper.net
{
    [Svn("$Id: db.cs 17583 2010-05-02 17:23:57Z david $")]
    class Db
    {
        public Db(string connectionString, string sql)
        {
            mCon = new SqlCeConnection();
            mCon.ConnectionString = Properties.Settings.Default.OodHelperConnectionString;
            mCmd = new SqlCeCommand(sql, mCon);
        }

        public Db(string sql)
        {
            mCon = new SqlCeConnection();
            mCon.ConnectionString = Properties.Settings.Default.OodHelperConnectionString;
            mCmd = new SqlCeCommand(sql, mCon);
        }

        public static void CreateDb()
        {
            string constr = Properties.Settings.Default.OodHelperConnectionString;

            if (File.Exists(".\\data\\oodhelper.sdf"))
            {
                File.Move(".\\data\\oodhelper.sdf", ".\\data\\oodhelper-" + DateTime.Now.Ticks.ToString() + ".sdf");
            }

            SqlCeEngine ce = new SqlCeEngine(constr);
            ce.CreateDatabase();
            ce.Dispose();

            SqlCeConnection con = new SqlCeConnection(constr);
            try
            {
                con.Open();
                SqlCeCommand cmd = con.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE [boats] (
  [bid] int NOT NULL IDENTITY (1,1)
, [id] int NULL
, [boatname] nvarchar(20) NULL
, [boatclass] nvarchar(20) NULL
, [sailno] nvarchar(8) NULL
, [dinghy] bit NULL
, [hulltype] nvarchar(1) NULL
, [distance] int NULL
, [crewname] nvarchar(30) NULL
, [open_handicap] int NULL
, [handicap_status] nvarchar(2) NULL
, [rolling_handicap] int NULL
, [crew_skill_factor] int NULL
, [small_cat_handicap_rating] numeric(4,3) NULL
, [engine_propeller] nvarchar(3) NULL
, [keel] nvarchar(2) NULL
, [deviations] nvarchar(30) NULL
, [subscription] nvarchar(26) NULL
, [boatmemo] ntext NULL
, [berth] nvarchar(6) NULL
, [hired] bit NULL
, [p] nvarchar(1) NULL
, [s] bit NULL
, [beaten] int NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [calendar] (
  [rid] int NOT NULL IDENTITY (1,1)
, [date] datetime NULL
, [day] nvarchar(3) NULL
, [class] nvarchar(20) NULL
, [event] nvarchar(34) NULL
, [start] nvarchar(5) NULL
, [gp] nvarchar(1) NULL
, [course] nvarchar(9) NULL
, [ood] nvarchar(30) NULL
, [venue] nvarchar(11) NULL
, [spec] nvarchar(1) NULL
, [hc] nvarchar(1) NULL
, [hc_ul] int NULL
, [hc_meth] nvarchar(1) NULL
, [vis] int NULL
, [flag] nvarchar(10) NULL
, [timelimit] nvarchar(5) NULL
, [extension] nvarchar(5) NULL
, [computer] int NULL
, [memo] ntext NULL
, [raced] bit NULL
, [app] bit NULL
, [p] bit NULL
, [standard_corrected_time] float NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [calendar_series_join] (
  [sid] int NOT NULL
, [rid] int NOT NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [people] (
  [id] int NOT NULL IDENTITY (1,1)
, [main_id] int NULL
, [firstname] nvarchar(20) NULL
, [surname] nvarchar(28) NULL
, [address1] nvarchar(30) NULL
, [address2] nvarchar(30) NULL
, [address3] nvarchar(30) NULL
, [address4] nvarchar(30) NULL
, [postcode] nvarchar(9) NULL
, [hometel] nvarchar(20) NULL
, [worktel] nvarchar(20) NULL
, [mobile] nvarchar(20) NULL
, [email] nvarchar(45) NULL
, [club] nvarchar(10) NULL
, [member] nvarchar(6) NULL
, [manmemo] ntext NULL
, [cp] bit NULL
, [s] bit NULL
, [novice] bit NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [races] (
  [rid] int NOT NULL
, [bid] int NOT NULL
, [date] datetime NULL
, [start] nvarchar(8) NULL
, [fincode] nvarchar(5) NULL
, [fintime] nvarchar(8) NULL
, [findate] datetime NULL
, [laps] int NULL
, [elapsed] int NULL
, [corrected] float NULL
, [standard_corrected] float NULL
, [handicap_status] nvarchar(2) NULL
, [open_handicap] int NULL
, [rolling_handicap] int NULL
, [achieved_handicap] int NULL
, [new_rolling_handicap] int NULL
, [place] int NULL
, [points] float NULL
, [override_points] float NULL
, [performance_index] int NULL
, [a] nvarchar(1) NULL
, [c] nvarchar(1) NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [series] (
  [sid] int NOT NULL IDENTITY (1,1)
, [sname] nvarchar(34) NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"ALTER TABLE [boats] ADD CONSTRAINT [PK_boats] PRIMARY KEY ([bid])";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [calendar] ADD CONSTRAINT [PK_calendar] PRIMARY KEY ([rid])";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [calendar_series_join] ADD CONSTRAINT [PK_calendar_series_join] PRIMARY KEY ([sid],[rid])";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [people] ADD CONSTRAINT [PK_people] PRIMARY KEY ([id])";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [races] ADD CONSTRAINT [PK_races] PRIMARY KEY ([rid],[bid])";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [series] ADD CONSTRAINT [PK_series] PRIMARY KEY ([sid])";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE INDEX [IX_date] ON [calendar] ([date] ASC)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE INDEX [IX_bid] ON [races] ([bid] ASC)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE INDEX [IX_rid] ON [races] ([rid] ASC)";
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }

        public int ExecuteNonQuery(Hashtable p)
        {
            try
            {
                mCmd.Parameters.Clear();
                if (p != null)
                {
                    foreach (string k in p.Keys)
                    {
                        mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                    }
                }
                mCon.Open();
                return mCmd.ExecuteNonQuery();
            }
            finally
            {
                if (mCon.State != ConnectionState.Closed)
                    mCon.Close();
            }
        }

        private SqlCeDataAdapter mAdapt;
        private SqlCeConnection mCon;
        private SqlCeCommand mCmd;

        public Object GetScalar(Hashtable p)
        {
            DataTable d = new DataTable();
            mCmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                }
            }
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            mAdapt.Fill(d);
            mCon.Close();
            if (d.Rows.Count > 0)
                return d.Rows[0][0];
            else
                return DBNull.Value;
        }

        public Hashtable GetHashtable(Hashtable p)
        {
            DataTable d = new DataTable();
            mCmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                }
            }
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            mAdapt.Fill(d);
            mCon.Close();
            Hashtable h = new Hashtable();
            if (d.Rows.Count > 0)
            {
                foreach (DataColumn c in d.Columns)
                    h[c.ColumnName] = d.Rows[0][c];
            }
            return h;
        }

        public void Fill(DataTable d, Hashtable p)
        {
            mCmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                }
            }
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            mAdapt.Fill(d);
            mCon.Close();
        }

        public DataTable GetData(Hashtable p)
        {
            DataTable t = new DataTable();
            mCmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                }
            }
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            mAdapt.Fill(t);
            mCon.Close();
            return t;
        }

        public int GetNextIdentity(string table, string column)
        {
            mCon.Open();
            try
            {
                SqlCeCommand cmd = mCon.CreateCommand();
                cmd.CommandText = @"SELECT autoinc_next
                    FROM information_schema.columns
                    WHERE table_name = @table
                    AND column_name = @column";
                cmd.Parameters.Add("table", table);
                cmd.Parameters.Add("column", column);
                long nextid = (long)cmd.ExecuteScalar();
                return (int)nextid;
            }
            finally
            {
                mCon.Close();
            }
        }

        public int Commit(DataTable d)
        {
            SqlCeCommandBuilder cmb = new SqlCeCommandBuilder(mAdapt);
            mAdapt.DeleteCommand = cmb.GetDeleteCommand();
            mAdapt.InsertCommand = cmb.GetInsertCommand();
            mAdapt.UpdateCommand = cmb.GetUpdateCommand();
            return mAdapt.Update(d);
        }

        public void Dispose()
        {
            if (mCon != null) mCon.Dispose();
            if (mCmd != null) mCmd.Dispose();
            if (mAdapt != null) mAdapt.Dispose();
        }

        public static void Compact()
        {
            SqlCeEngine ce = new SqlCeEngine();
            ce.Compact(Properties.Settings.Default.OodHelperConnectionString);
            ce.Dispose();
        }
    }
}
