using System;
using System.Reflection;

namespace OodHelper.Data
{
    /// <summary>
    /// Owns the SQLite database file location and connection string for the application database.
    /// The database is a single file under LocalApplicationData; EF Core owns the schema (code-first
    /// migrations), so there is no separate master catalog or admin connection.
    /// </summary>
    internal static class SqliteConfig
    {
        public const string DatabaseName = "OodHelper";

        /// <summary>Folder under LocalApplicationData holding the SQLite database file.</summary>
        public static string DatabaseFolder { get; }

        /// <summary>Full path to the application SQLite database file.</summary>
        public static string DataFileName { get; }

        /// <summary>Connection to the application database; used by EF Core registration and the design-time factory.</summary>
        public static string ConnectionString { get; }

        static SqliteConfig()
        {
            AssemblyName an = Assembly.GetAssembly(typeof(App))!.GetName();
            DatabaseFolder =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{an.Name}\data";
            DataFileName = $@"{DatabaseFolder}\{DatabaseName}.db";
            ConnectionString = $"Data Source={DataFileName}";
        }
    }
}
