using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace OodHelper
{
    class Db : IDisposable
    {
        private static string DatabaseName = "OodHelper";
        private static string DatabaseFolder;
        private static string DataFileName;
        private static string LogFileName;
        private static string _DatabaseConstr;

        public static string DatabaseConstr { get { return _DatabaseConstr; } }

        static Db()
        {
            Assembly _ass = Assembly.GetAssembly(typeof(App));
            AssemblyName _an = _ass.GetName();
            DatabaseFolder = string.Format(@"{0}\{1}\data", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _an.Name);

            DataFileName = string.Format(@"{0}\{1}.mdf", DatabaseFolder, DatabaseName);
            LogFileName = string.Format(@"{0}\{1}.ldf", DatabaseFolder, DatabaseName);
            _DatabaseConstr = string.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog={1};Integrated Security=True;", DataFileName, DatabaseName);
        }

        public Db(string connectionString, string sql)
        {
            mCon = new SqlConnection();
            mCon.ConnectionString = connectionString;
            mCmd = new SqlCommand(sql, mCon);
        }

        public Db(string sql)
        {
            mCon = new SqlConnection();
            mCon.ConnectionString = DatabaseConstr;
            if (!File.Exists(DataFileName))
            {
                Db.CreateDb();
            }
            mCmd = new SqlCommand(sql, mCon);
        }

        public static bool CreateDatabase(string dbName, string dbFileName)
        {
            try
            {
                string connectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand();

                    cmd.CommandText = String.Format("CREATE DATABASE [{0}] ON (NAME = N'{0}', FILENAME = '{1}')", dbName, dbFileName);
                    cmd.ExecuteNonQuery();
                }

                if (File.Exists(dbFileName)) return true;
                else return false;
            }
            catch
            {
                throw;
            }
        }

        public static bool DetachDatabase(string dbName)
        {
            try
            {
                string connectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = String.Format("exec sp_detach_db '{0}'", dbName);
                    cmd.ExecuteNonQuery();

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void CreateDb()
        {

            string constr = Db.DatabaseConstr;

            if (!Directory.Exists(DatabaseFolder))
                Directory.CreateDirectory(DatabaseFolder);

            if (File.Exists(DataFileName))
            {
                DetachDatabase(DatabaseName);
                string _backupDb = string.Format(@"\{0}-{1}", DatabaseName, DateTime.Now.Ticks.ToString());
                File.Move(DataFileName, DatabaseFolder + _backupDb + ".mdf");
                File.Move(LogFileName, DatabaseFolder + _backupDb + ".ldf");
            }

            CreateDatabase(DatabaseName, DataFileName);

            SqlConnection con = new SqlConnection(constr);
            try
            {
                con.Open();
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandText = @"
/****** Object:  Table [dbo].[boat_crew]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[boat_crew](
	[id] [int] NOT NULL,
	[bid] [int] NOT NULL,
 CONSTRAINT [PK_boat_crew] PRIMARY KEY CLUSTERED 
(
	[id] ASC,
	[bid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[boats]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[boats](
	[bid] [int] IDENTITY(1,1) NOT NULL,
	[id] [int] NULL,
	[boatname] [nvarchar](20) NULL,
	[boatclass] [nvarchar](20) NULL,
	[sailno] [nvarchar](8) NULL,
	[dinghy] [bit] NULL,
	[hulltype] [nvarchar](1) NULL,
	[distance] [int] NULL,
	[crewname] [nvarchar](30) NULL,
	[open_handicap] [int] NULL,
	[handicap_status] [nvarchar](2) NULL,
	[rolling_handicap] [int] NULL,
	[crew_skill_factor] [int] NULL,
	[small_cat_handicap_rating] [numeric](4, 3) NULL,
	[engine_propeller] [nvarchar](3) NULL,
	[keel] [nvarchar](2) NULL,
	[deviations] [nvarchar](30) NULL,
	[subscription] [nvarchar](26) NULL,
	[boatmemo] [ntext] NULL,
	[berth] [nvarchar](6) NULL,
	[hired] [bit] NULL,
	[p] [nvarchar](1) NULL,
	[s] [bit] NULL,
	[beaten] [int] NULL,
	[uid] [uniqueidentifier] NULL,
 CONSTRAINT [PK_boats] PRIMARY KEY CLUSTERED 
(
	[bid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[calendar]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[calendar](
	[rid] [int] IDENTITY(1,1) NOT NULL,
	[start_date] [datetime] NULL,
	[time_limit_type] [nvarchar](1) NULL,
	[time_limit_fixed] [datetime] NULL,
	[time_limit_delta] [int] NULL,
	[extension] [int] NULL,
	[class] [nvarchar](20) NULL,
	[event] [nvarchar](34) NULL,
	[price_code] [nvarchar](1) NULL,
	[course] [nvarchar](9) NULL,
	[ood] [nvarchar](30) NULL,
	[venue] [nvarchar](11) NULL,
	[racetype] [nvarchar](20) NULL,
	[handicapping] [nvarchar](1) NULL,
	[visitors] [int] NULL,
	[flag] [nvarchar](20) NULL,
	[memo] [ntext] NULL,
	[is_race] [bit] NULL,
	[raced] [bit] NULL,
	[approved] [bit] NULL,
	[course_choice] [nvarchar](10) NULL,
	[laps_completed] [int] NULL,
	[wind_speed] [nvarchar](10) NULL,
	[wind_direction] [nvarchar](10) NULL,
	[standard_corrected_time] [float] NULL,
	[result_calculated] [datetime] NULL,
 CONSTRAINT [PK_calendar] PRIMARY KEY CLUSTERED 
(
	[rid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[calendar_series_join]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[calendar_series_join](
	[sid] [int] NOT NULL,
	[rid] [int] NOT NULL,
 CONSTRAINT [PK_calendar_series_join] PRIMARY KEY CLUSTERED 
(
	[sid] ASC,
	[rid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[people]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[people](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[main_id] [int] NULL,
	[firstname] [nvarchar](20) NULL,
	[surname] [nvarchar](28) NULL,
	[address1] [nvarchar](30) NULL,
	[address2] [nvarchar](30) NULL,
	[address3] [nvarchar](30) NULL,
	[address4] [nvarchar](30) NULL,
	[postcode] [nvarchar](9) NULL,
	[hometel] [nvarchar](20) NULL,
	[worktel] [nvarchar](20) NULL,
	[mobile] [nvarchar](20) NULL,
	[email] [nvarchar](45) NULL,
	[club] [nvarchar](10) NULL,
	[member] [nvarchar](6) NULL,
	[manmemo] [ntext] NULL,
	[cp] [bit] NULL,
	[s] [bit] NULL,
	[novice] [bit] NULL,
	[uid] [uniqueidentifier] NULL,
	[papernewsletter] [bit] NULL,
	[handbookexclude] [bit] NULL,
 CONSTRAINT [PK_people] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[portsmouth_numbers]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[portsmouth_numbers](
	[id] [uniqueidentifier] NOT NULL,
	[class_name] [nvarchar](100) NULL,
	[no_of_crew] [int] NULL,
	[rig] [nvarchar](1) NULL,
	[spinnaker] [nvarchar](1) NULL,
	[engine] [nvarchar](3) NULL,
	[keel] [nvarchar](1) NULL,
	[number] [int] NULL,
	[status] [nvarchar](1) NULL,
	[notes] [ntext] NULL,
 CONSTRAINT [PK_portsmouth_numbers] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[races]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[races](
	[rid] [int] NOT NULL,
	[bid] [int] NOT NULL,
	[start_date] [datetime] NULL,
	[finish_code] [nvarchar](5) NULL,
	[finish_date] [datetime] NULL,
	[interim_date] [datetime] NULL,
	[last_edit] [datetime] NULL,
	[laps] [int] NULL,
	[place] [int] NULL,
	[points] [float] NULL,
	[override_points] [float] NULL,
	[elapsed] [int] NULL,
	[corrected] [float] NULL,
	[standard_corrected] [float] NULL,
	[handicap_status] [nvarchar](2) NULL,
	[open_handicap] [int] NULL,
	[rolling_handicap] [int] NULL,
	[achieved_handicap] [int] NULL,
	[new_rolling_handicap] [int] NULL,
	[performance_index] [int] NULL,
	[a] [nvarchar](1) NULL,
	[c] [nvarchar](1) NULL,
 CONSTRAINT [PK_races] PRIMARY KEY NONCLUSTERED 
(
	[rid] ASC,
	[bid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[select_rules]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[select_rules](
	[id] [uniqueidentifier] NOT NULL,
	[name] [nvarchar](255) NULL,
	[parent] [uniqueidentifier] NULL,
	[application] [int] NULL,
	[field] [nvarchar](255) NULL,
	[condition] [int] NULL,
	[string_value] [nvarchar](255) NULL,
	[number_bound1] [numeric](18, 4) NULL,
	[number_bound2] [numeric](18, 4) NULL,
 CONSTRAINT [PK_select_rule] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[series]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[series](
	[sid] [int] IDENTITY(1,1) NOT NULL,
	[sname] [nvarchar](255) NOT NULL,
	[discards] [nvarchar](255) NULL,
 CONSTRAINT [PK_series] PRIMARY KEY CLUSTERED 
(
	[sid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[series_results]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[series_results](
	[sid] [int] NOT NULL,
	[bid] [int] NOT NULL,
	[division] [nvarchar](20) NOT NULL,
	[entered] [int] NULL,
	[gross] [float] NULL,
	[nett] [float] NULL,
	[place] [int] NULL,
 CONSTRAINT [PK_series_results] PRIMARY KEY CLUSTERED 
(
	[sid] ASC,
	[division] ASC,
	[bid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[updates]    Script Date: 28/03/2013 05:54:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[updates](
	[dummy] [int] NULL,
	[upload] [datetime] NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[boats]  WITH CHECK ADD  CONSTRAINT [FK_boats_people] FOREIGN KEY([id])
REFERENCES [dbo].[people] ([id])
ON UPDATE CASCADE
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[boats] CHECK CONSTRAINT [FK_boats_people]
GO
ALTER TABLE [dbo].[calendar_series_join]  WITH CHECK ADD  CONSTRAINT [FK_calendar_series_join_calendar] FOREIGN KEY([sid])
REFERENCES [dbo].[calendar] ([rid])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[calendar_series_join] CHECK CONSTRAINT [FK_calendar_series_join_calendar]
GO
ALTER TABLE [dbo].[calendar_series_join]  WITH CHECK ADD  CONSTRAINT [FK_calendar_series_join_series] FOREIGN KEY([sid])
REFERENCES [dbo].[series] ([sid])
GO
ALTER TABLE [dbo].[calendar_series_join] CHECK CONSTRAINT [FK_calendar_series_join_series]
GO
ALTER TABLE [dbo].[races]  WITH CHECK ADD  CONSTRAINT [FK_races_boats] FOREIGN KEY([bid])
REFERENCES [dbo].[boats] ([bid])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[races] CHECK CONSTRAINT [FK_races_boats]
GO
ALTER TABLE [dbo].[races]  WITH CHECK ADD  CONSTRAINT [FK_races_calendar] FOREIGN KEY([rid])
REFERENCES [dbo].[calendar] ([rid])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[races] CHECK CONSTRAINT [FK_races_calendar]
GO
";
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

        private SqlDataAdapter mAdapt;
        private SqlConnection mCon;
        private SqlCommand mCmd;

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
            mAdapt = new SqlDataAdapter(mCmd);
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
                        mCmd.Parameters.Add(new SqlParameter(k, DBNull.Value));
                    else
                        mCmd.Parameters.Add(new SqlParameter(k, p[k]));
                }
            }
        }

        public Hashtable GetHashtable(Hashtable p)
        {
            DataTable d = new DataTable();
            addCommandParameters(p);
            mAdapt = new SqlDataAdapter(mCmd);
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
            mAdapt = new SqlDataAdapter(mCmd);
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
            mAdapt = new SqlDataAdapter(mCmd);
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

        public int GetNextIdentity(string table)
        {
            mCon.Open();
            try
            {
                SqlCommand cmd = mCon.CreateCommand();
                cmd.CommandText = @"SELECT IDENT_CURRENT(@table)";
                cmd.Parameters.AddWithValue("table", table);
                
                decimal _nextId = (decimal) cmd.ExecuteScalar();
                return (int)_nextId + 1;
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
                //
                // After compacting we need to adjust seed values on identity columns
                //
                ReseedDatabase();
            }
            catch (SqlException)
            {
            }
        }

        public static void ReseedDatabase()
        {
            string o;
            int b = 1, t;
            b = Settings.BottomSeed;
            t = Settings.TopSeed;

            ReseedTable("boats", "bid", b, t);
            ReseedTable("people", "id", b, t);
            ReseedTable("calendar", "rid");
            ReseedTable("series", "sid");
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
                s = new Db(string.Format("DBCC CHECKIDENT ({0}, RESEED, {1})", tname, seedvalue));
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
            s = new Db(string.Format("DBCC CHECKIDENT ({0}, RESEED, {1})", tname, seedvalue));
            s.ExecuteNonQuery(null);
        }
    }
}