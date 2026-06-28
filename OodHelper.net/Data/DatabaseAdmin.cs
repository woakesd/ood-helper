using System;
using System.IO;
using Microsoft.Data.SqlClient;

namespace OodHelper.Data
{
    /// <summary>
    /// Administrative operations on the LocalDB application database — first-run creation, backup,
    /// single/multi-user toggling and identity reseeding. Extracted from the retired <c>Db</c>
    /// ADO.NET helper; opens its own short-lived connections from <see cref="LocalDbConfig"/>.
    /// </summary>
    internal static class DatabaseAdmin
    {
        /// <summary>Creates the database on first run (when the .mdf does not yet exist).</summary>
        public static void EnsureDatabaseExists()
        {
            if (!File.Exists(LocalDbConfig.DataFileName))
                CreateDb();
        }

        public static void SetSingleUser(string dbName)
        {
            using (var conn = new SqlConnection(LocalDbConfig.MasterConnectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = $"ALTER DATABASE [{dbName}] SET SINGLE_USER";
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
            using (var conn = new SqlConnection(LocalDbConfig.MasterConnectionString))
            {
                try
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = $"ALTER DATABASE [{dbName}] SET MULTI_USER";
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
            using (var conn = new SqlConnection(LocalDbConfig.MasterConnectionString))
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
            using (var connection = new SqlConnection(LocalDbConfig.MasterConnectionString))
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
            string constr = LocalDbConfig.ConnectionString;

            if (!Directory.Exists(LocalDbConfig.DatabaseFolder))
                Directory.CreateDirectory(LocalDbConfig.DatabaseFolder);

            if (File.Exists(LocalDbConfig.DataFileName))
            {
                SetSingleUser(LocalDbConfig.DatabaseName);
                BackupDatabase(LocalDbConfig.DatabaseName, LocalDbConfig.DatabaseFolder);
            }

            CreateDatabase(LocalDbConfig.DatabaseName, LocalDbConfig.DataFileName);

            var con = new SqlConnection(constr);
            try
            {
                con.Open();
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE [dbo].[boats](
	[bid] [int] IDENTITY(1,1) NOT NULL,
	[boatname] [nvarchar](20) NULL,
	[boatclass] [nvarchar](20) NULL,
	[sailno] [nvarchar](8) NULL,
	[dinghy] [bit] NULL,
	[hulltype] [nvarchar](1) NULL,
	[distance] [int] NULL,
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

        public static void ReseedDatabase()
        {
            var b = Settings.BottomSeed;
            var t = Settings.TopSeed;

            ReseedTable("boats", "bid", b, t);
            ReseedTable("calendar", "rid");
            ReseedTable("series", "sid");
        }

        private static void ReseedTable(string tname, string ident, int b, int t)
        {
            if (b < t && b != 0 && t != 0)
            {
                int seedvalue;
                using (var con = new SqlConnection(LocalDbConfig.ConnectionString))
                {
                    con.Open();
                    var cmd = con.CreateCommand();
                    cmd.CommandText = $"SELECT MAX({ident}) FROM {tname} WHERE {ident} BETWEEN @b AND @t";
                    cmd.Parameters.AddWithValue("b", b);
                    cmd.Parameters.AddWithValue("t", t);
                    object o = cmd.ExecuteScalar();
                    seedvalue = o != null && o != DBNull.Value ? ((int) o) + 1 : b;
                }
                CheckIdentReseed(tname, seedvalue);
            }
        }

        private static void ReseedTable(string tname, string ident)
        {
            int seedvalue;
            using (var con = new SqlConnection(LocalDbConfig.ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = $"SELECT MAX({ident}) FROM {tname}";
                object o = cmd.ExecuteScalar();
                seedvalue = o != null && o != DBNull.Value ? ((int) o) + 1 : 1;
            }
            CheckIdentReseed(tname, seedvalue);
        }

        private static void CheckIdentReseed(string tname, int seedvalue)
        {
            using (var con = new SqlConnection(LocalDbConfig.ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = $"DBCC CHECKIDENT ({tname}, RESEED, {seedvalue})";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
