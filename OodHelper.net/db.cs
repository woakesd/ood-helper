using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;

namespace OodHelper
{
    class Db : IDisposable
    {
        private static string DatabaseFolder = string.Format(@"{0}\data", AppDomain.CurrentDomain.BaseDirectory);
        private static string DatabaseName = string.Format(@"{0}\oodhelper.sdf", DatabaseFolder);
        private static string _DatabaseConstr = string.Format(@"Data Source={0}", DatabaseName);

        public static string DatabaseConstr { get { return _DatabaseConstr; } }
        
        public Db(string connectionString, string sql)
        {
            mCon = new SqlCeConnection();
            mCon.ConnectionString = connectionString;
            mCmd = new SqlCeCommand(sql, mCon);
        }

        public Db(string sql)
        {
            mCon = new SqlCeConnection();
            mCon.ConnectionString = DatabaseConstr;
            if (!File.Exists(DatabaseName))
            {
                Db.CreateDb();
            }
            mCmd = new SqlCeCommand(sql, mCon);
        }

        public static void CreateDb()
        {
            string constr = Db.DatabaseConstr;

            if (!Directory.Exists(DatabaseFolder))
                Directory.CreateDirectory(DatabaseFolder);

            if (File.Exists(DatabaseName))
            {
                File.Move(DatabaseName, DatabaseFolder + @"\oodhelper-" + DateTime.Now.Ticks.ToString() + ".sdf");
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
, [uid] uniqueidentifier NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [calendar] (
  [rid] int NOT NULL IDENTITY (1,1)
, [start_date] datetime NULL
, [time_limit_type] nvarchar(1) NULL
, [time_limit_fixed] datetime NULL
, [time_limit_delta] int NULL
, [extension] int NULL
, [class] nvarchar(20) NULL
, [event] nvarchar(34) NULL
, [price_code] nvarchar(1) NULL
, [course] nvarchar(9) NULL
, [ood] nvarchar(30) NULL
, [venue] nvarchar(11) NULL
, [average_lap] bit NULL
, [timegate] bit NULL
, [sternchase] bit NULL
, [handicapping] nvarchar(1) NULL
, [visitors] int NULL
, [flag] nvarchar(10) NULL
, [memo] ntext NULL
, [is_race] bit NULL
, [raced] bit NULL
, [approved] bit NULL
, [course_choice] nvarchar(10) NULL
, [laps_completed] int NULL
, [wind_speed] nvarchar(10) NULL
, [wind_direction] nvarchar(10) NULL
, [standard_corrected_time] float NULL
, [result_calculated] datetime NULL
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
, [uid] uniqueidentifier NULL
, [papernewsletter] bit NULL
, [handbookexclude] bit NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [races] (
  [rid] int NOT NULL
, [bid] int NOT NULL
, [start_date] datetime NULL
, [finish_code] nvarchar(5) NULL
, [finish_date] datetime NULL
, [finish_date_2] datetime NULL
, [last_edit] datetime NULL
, [laps] int NULL
, [place] int NULL
, [points] float NULL
, [override_points] float NULL
, [elapsed] int NULL
, [corrected] float NULL
, [standard_corrected] float NULL
, [handicap_status] nvarchar(2) NULL
, [open_handicap] int NULL
, [rolling_handicap] int NULL
, [achieved_handicap] int NULL
, [new_rolling_handicap] int NULL
, [performance_index] int NULL
, [a] nvarchar(1) NULL
, [c] nvarchar(1) NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [series] (
  [sid] int NOT NULL IDENTITY (1,1)
, [sname] nvarchar(255) NOT NULL
, [discards] nvarchar(255) NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [series_results] (
  [sid] int not null
, [bid] int not null
, [division] nvarchar(20) not null
, [entered] int null
, [gross] float null
, [nett] float null
, [place] int
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [updates] (
  [dummy] int
, [upload] DATETIME NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [select_rules] (
  [id] uniqueidentifier NOT NULL
, [name] nvarchar(255) NULL
, [parent] uniqueidentifier NULL
, [application] int NULL
, [field] nvarchar(255) NULL
, [condition] int NULL
, [string_value] nvarchar(255) NULL
, [number_bound1] numeric(18,4) NULL
, [number_bound2] numeric(18,4) NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [boat_crew] (
  [id] int NOT NULL
, [bid] int NOT NULL
)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
CREATE TABLE [portsmouth_numbers] (
  [id] uniqueidentifier NOT NULL
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
                cmd.CommandText = @"CREATE INDEX [IX_date] ON [calendar] ([start_date] ASC)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE INDEX [IX_bid] ON [races] ([bid] ASC)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE INDEX [IX_rid] ON [races] ([rid] ASC)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [series_results] ADD CONSTRAINT [PK_series_results] PRIMARY KEY ([sid],[division],[bid])";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [select_rules] ADD CONSTRAINT [PK_select_rule] PRIMARY KEY ([id])";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE INDEX [IX_select_rule_parent] ON [select_rules] ([parent] ASC)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [boat_crew] ADD CONSTRAINT [PK_boat_crew] PRIMARY KEY ([id],[bid])";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [portsmouth_numbers] ADD CONSTRAINT [PK_portsmouth_numbers] PRIMARY KEY ([id])";
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
                addCommandParameters(p);
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

        public IDbConnection Connection
        {
            get
            {
                return mCon;
            }
        }

        public Object GetScalar(Hashtable p)
        {
            DataTable d = new DataTable();
            addCommandParameters(p);
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            try
            {
                mAdapt.Fill(d);
            }
            finally
            {
                mCon.Close();
            }

            if (d.Rows.Count > 0)
                return d.Rows[0][0];
            else
                return DBNull.Value;
        }

        private void addCommandParameters(Hashtable p)
        {
            mCmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    if (p[k] == null || (p[k]).GetType() == typeof(string) && p[k] as string == string.Empty)
                        mCmd.Parameters.Add(new SqlCeParameter(k, DBNull.Value));
                    else
                        mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                }
            }
        }

        public Hashtable GetHashtable(Hashtable p)
        {
            DataTable d = new DataTable();
            addCommandParameters(p);
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            try
            {
                mAdapt.Fill(d);
            }
            finally
            {
                mCon.Close();
            }

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
            addCommandParameters(p);
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            try
            {
                mAdapt.Fill(d);
            }
            finally
            {
                mCon.Close();
            }
        }

        public DataTable GetData(Hashtable p)
        {
            DataTable t = new DataTable();
            addCommandParameters(p);
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            try
            {
                mAdapt.Fill(t);
            }
            finally
            {
                mCon.Close();
            }
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

        public void Dispose()
        {
            if (mCon != null) mCon.Dispose();
            if (mCmd != null) mCmd.Dispose();
            if (mAdapt != null) mAdapt.Dispose();
        }

        public static void Compact()
        {
            try
            {
                Properties.Settings s = new Properties.Settings();
                SqlCeEngine ce = new SqlCeEngine();
                ce.LocalConnectionString = DatabaseConstr;
                ce.Compact(DatabaseConstr);
                ce.Dispose();

                //
                // After compacting we need to adjust seed values on identity columns
                //
                ReseedDatabase();
            }
            catch (SqlCeException)
            {
            }
        }

        public static void ReseedDatabase()
        {
            Object o;
            int b, t;
            if ((o = DbSettings.GetSetting("bottomseed")) != null)
                b = (int)o;
            else
                b = 1;
            if ((o = DbSettings.GetSetting("topseed")) != null)
            {
                t = (int)o;
                ReseedTable("boats", "bid", b, t);
                ReseedTable("people", "id", b, t);
                ReseedTable("calendar", "rid");
                ReseedTable("series", "sid");
            }
            else
            {
                ReseedTable("boats", "bid");
                ReseedTable("people", "id");
                ReseedTable("calendar", "rid");
                ReseedTable("series", "sid");
            }
        }

        private static void ReseedTable(string tname, string ident, int b, int t)
        {
            if (b < t && b != 0 && t != 0)
            {
                Db s = new Db("SELECT MAX(" + ident + ") " +
                    "FROM " + tname + " " +
                    "WHERE " + ident + " BETWEEN @b AND @_task");
                Hashtable p = new Hashtable();
                p["b"] = b;
                p["_task"] = t;
                object o;
                int seedvalue;
                if ((o = s.GetScalar(p)) != DBNull.Value)
                {
                    seedvalue = ((int)o) + 1;
                }
                else
                {
                    seedvalue = b;
                }
                s = new Db("ALTER TABLE " + tname + " " +
                    "ALTER COLUMN " + ident + " IDENTITY(" + seedvalue.ToString() + ",1)");
                s.ExecuteNonQuery(null);
            }
        }

        private static void ReseedTable(string tname, string ident)
        {
            Db s = new Db("SELECT MAX(" + ident + ") " +
                "FROM " + tname);
            object o;
            int seedvalue;
            if ((o = s.GetScalar(null)) != DBNull.Value)
            {
                seedvalue = ((int)o) + 1;
            }
            else
            {
                seedvalue = 1;
            }
            s = new Db("ALTER TABLE " + tname + " " +
                "ALTER COLUMN " + ident + " IDENTITY(" + seedvalue.ToString() + ",1)");
            s.ExecuteNonQuery(null);
        }
    }
}
