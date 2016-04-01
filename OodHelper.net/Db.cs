using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace OodHelper
{
    internal class Db : IDisposable
    {
        private const string MasterConnection =
            @"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True";

        private const string DatabaseName = "OodHelper";
        private static readonly string DatabaseFolder;
        private static readonly string DataFileName;
        private readonly SqlConnection _con;
        private SqlDataAdapter _adapt;
        private SqlCommand _cmd;

        static Db()
        {
            Assembly ass = Assembly.GetAssembly(typeof (App));
            AssemblyName an = ass.GetName();
            DatabaseFolder = string.Format(@"{0}\{1}\data",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), an.Name);

            DataFileName = string.Format(@"{0}\{1}.mdf", DatabaseFolder, DatabaseName);
            DatabaseConstr = string.Format(
                @"Data Source=(LocalDB)\v11.0;Initial Catalog={0};Integrated Security=True;", DatabaseName);
        }

        public Db(string sqlCommand) : this()
        {
            Sql = sqlCommand;
        }

        public Db()
        {
            _con = new SqlConnection {ConnectionString = DatabaseConstr};
            if (!File.Exists(DataFileName))
            {
                CreateDb();
            }
        }

        public static string DatabaseConstr { get; private set; }

        public string Sql
        {
            set
            {
                _cmd = _con.CreateCommand();
                _cmd.CommandText = value;
            }

            get { return _cmd.CommandText; }
        }

        public IDbConnection Connection
        {
            get { return _con; }
        }

        public void Dispose()
        {
            if (_cmd != null) _cmd.Dispose();
            if (_con != null) _con.Dispose();
        }

        public static void SetSingleUser(string dbName)
        {
            using (var conn = new SqlConnection(MasterConnection))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                        dbName);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = string.Format("ALTER DATABASE [{0}] SET SINGLE_USER", dbName);
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static void SetMultiUser(string dbName)
        {
            using (var conn = new SqlConnection(MasterConnection))
            {
                try
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("ALTER DATABASE [{0}] SET MULTI_USER", dbName);
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static bool CreateDatabase(string dbName, string dbFileName)
        {
            using (var conn = new SqlConnection(MasterConnection))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = string.Format("IF DB_ID('{0}') IS NOT NULL DROP DATABASE [{0}]", dbName);
                cmd.ExecuteNonQuery();

                cmd.CommandText = string.Format("CREATE DATABASE [{0}] ON (NAME = N'{0}', FILENAME = '{1}')", dbName,
                    dbFileName);
                cmd.ExecuteNonQuery();
            }

            if (File.Exists(dbFileName)) return true;
            return false;
        }

        public static void BackupDatabase(string dbName, string location)
        {
            using (var connection = new SqlConnection(MasterConnection))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText =
                        String.Format(@"BACKUP DATABASE [{0}] TO  DISK = N'{1}\{0}.bak' WITH NOFORMAT, INIT,  NAME = N'{0}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD;
declare @backupSetId as int
select @backupSetId = position from msdb..backupset where database_name=N'{0}' and backup_set_id=(select max(backup_set_id) from msdb..backupset where database_name=N'{0}' )
if @backupSetId is null begin raiserror(N'Verify failed. Backup information for database ''{0}'' not found.', 16, 1) end
RESTORE VERIFYONLY FROM  DISK = N'{1}\{0}.bak' WITH  FILE = @backupSetId,  NOUNLOAD,  NOREWIND;", dbName, location);
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public static void CreateDb()
        {
            string constr = DatabaseConstr;

            if (!Directory.Exists(DatabaseFolder))
                Directory.CreateDirectory(DatabaseFolder);

            if (File.Exists(DataFileName))
            {
                SetSingleUser(DatabaseName);
                BackupDatabase(DatabaseName, DatabaseFolder);
            }

            CreateDatabase(DatabaseName, DataFileName);

            var con = new SqlConnection(constr);
            try
            {
                con.Open();
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE [dbo].[boat_crew](
	[id] [int] NOT NULL,
	[bid] [int] NOT NULL,
 CONSTRAINT [PK_boat_crew] PRIMARY KEY CLUSTERED 
(
	[id] ASC,
	[bid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

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
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

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
	[course] [nvarchar](15) NULL,
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
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[calendar_series_join](
	[sid] [int] NOT NULL,
	[rid] [int] NOT NULL,
 CONSTRAINT [PK_calendar_series_join] PRIMARY KEY CLUSTERED 
(
	[sid] ASC,
	[rid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

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
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

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
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[races](
	[rid] [int] NOT NULL,
	[bid] [int] NOT NULL,
	[start_date] [datetime] NULL,
	[finish_code] [nvarchar](5) NULL,
	[finish_date] [datetime] NULL,
	[interim_date] [datetime] NULL,
    [restricted_sail] [bit] NULL,
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
) ON [PRIMARY];

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
) ON [PRIMARY];

CREATE TABLE [dbo].[series](
	[sid] [int] IDENTITY(1,1) NOT NULL,
	[sname] [nvarchar](255) NOT NULL,
	[discards] [nvarchar](255) NULL,
 CONSTRAINT [PK_series] PRIMARY KEY CLUSTERED 
(
	[sid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

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
) ON [PRIMARY];

CREATE TABLE [dbo].[updates](
	[dummy] [int] NULL,
	[upload] [datetime] NULL
) ON [PRIMARY];

ALTER TABLE [dbo].[boats]  WITH CHECK ADD  CONSTRAINT [FK_boats_people] FOREIGN KEY([id])
REFERENCES [dbo].[people] ([id])
ON UPDATE CASCADE
ON DELETE SET NULL;

ALTER TABLE [dbo].[boats] CHECK CONSTRAINT [FK_boats_people];

ALTER TABLE [dbo].[calendar_series_join]  WITH CHECK ADD  CONSTRAINT [FK_calendar_series_join_calendar] FOREIGN KEY([rid])
REFERENCES [dbo].[calendar] ([rid])
ON UPDATE CASCADE
ON DELETE CASCADE;

ALTER TABLE [dbo].[calendar_series_join] CHECK CONSTRAINT [FK_calendar_series_join_calendar];

ALTER TABLE [dbo].[calendar_series_join]  WITH CHECK ADD  CONSTRAINT [FK_calendar_series_join_series] FOREIGN KEY([sid])
REFERENCES [dbo].[series] ([sid]);

ALTER TABLE [dbo].[calendar_series_join] CHECK CONSTRAINT [FK_calendar_series_join_series];

ALTER TABLE [dbo].[races]  WITH CHECK ADD  CONSTRAINT [FK_races_boats] FOREIGN KEY([bid])
REFERENCES [dbo].[boats] ([bid])
ON UPDATE CASCADE
ON DELETE CASCADE;

ALTER TABLE [dbo].[races] CHECK CONSTRAINT [FK_races_boats];

ALTER TABLE [dbo].[races]  WITH CHECK ADD  CONSTRAINT [FK_races_calendar] FOREIGN KEY([rid])
REFERENCES [dbo].[calendar] ([rid])
ON UPDATE CASCADE
ON DELETE CASCADE;

ALTER TABLE [dbo].[races] CHECK CONSTRAINT [FK_races_calendar];
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
                AddCommandParameters(p);
                _con.Open();
                return _cmd.ExecuteNonQuery();
            }
            finally
            {
                if (_con.State != ConnectionState.Closed)
                    _con.Close();
            }
        }

        public Object GetScalar(Hashtable p)
        {
            var d = new DataTable();
            AddCommandParameters(p);
            _adapt = new SqlDataAdapter(_cmd);
            _con.Open();
            try
            {
                _adapt.Fill(d);
            }
            finally
            {
                _con.Close();
            }

            if (d.Rows.Count > 0)
                return d.Rows[0][0];
            return DBNull.Value;
        }

        private void AddCommandParameters(Hashtable p)
        {
            _cmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    if (p[k] == null || p[k] is string && p[k] as string == string.Empty)
                        _cmd.Parameters.Add(new SqlParameter(k, DBNull.Value));
                    else
                        _cmd.Parameters.Add(new SqlParameter(k, p[k]));
                }
            }
        }

        public Hashtable GetHashtable(Hashtable p)
        {
            var d = new DataTable();
            AddCommandParameters(p);
            _adapt = new SqlDataAdapter(_cmd);
            _con.Open();
            try
            {
                _adapt.Fill(d);
            }
            finally
            {
                _con.Close();
            }

            var h = new Hashtable();
            if (d.Rows.Count > 0)
            {
                foreach (DataColumn c in d.Columns)
                    h[c.ColumnName] = d.Rows[0][c];
            }
            return h;
        }

        public void Fill(DataTable d, Hashtable p)
        {
            AddCommandParameters(p);
            _adapt = new SqlDataAdapter(_cmd);
            _con.Open();
            try
            {
                _adapt.Fill(d);
            }
            finally
            {
                _con.Close();
            }
        }

        public DataTable GetData(Hashtable p)
        {
            var t = new DataTable();
            AddCommandParameters(p);
            _adapt = new SqlDataAdapter(_cmd);
            _con.Open();
            try
            {
                _adapt.Fill(t);
            }
            finally
            {
                _con.Close();
            }
            return t;
        }

        public int GetNextIdentity(string table)
        {
            _con.Open();
            try
            {
                SqlCommand cmd = _con.CreateCommand();
                cmd.CommandText = @"SELECT IDENT_CURRENT(@table)";
                cmd.Parameters.AddWithValue("table", table);

                var nextId = (decimal) cmd.ExecuteScalar();
                return (int) nextId + 1;
            }
            finally
            {
                _con.Close();
            }
        }

        public static void ReseedDatabase()
        {
            var b = Settings.BottomSeed;
            var t = Settings.TopSeed;

            ReseedTable("boats", "bid", b, t);
            ReseedTable("people", "id", b, t);
            ReseedTable("calendar", "rid");
            ReseedTable("series", "sid");
        }

        private static void ReseedTable(string tname, string ident, int b, int t)
        {
            if (b < t && b != 0 && t != 0)
            {
                var s = new Db("SELECT MAX(" + ident + ") " +
                               "FROM " + tname + " " +
                               "WHERE " + ident + " BETWEEN @b AND @_task");
                var p = new Hashtable();
                p["b"] = b;
                p["_task"] = t;
                object o;
                int seedvalue;
                if ((o = s.GetScalar(p)) != DBNull.Value)
                {
                    seedvalue = ((int) o) + 1;
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
            var s = new Db("SELECT MAX(" + ident + ") " +
                           "FROM " + tname);
            object o;
            int seedvalue;
            if ((o = s.GetScalar(null)) != DBNull.Value)
            {
                seedvalue = ((int) o) + 1;
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