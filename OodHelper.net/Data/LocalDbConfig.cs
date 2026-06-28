using System;
using System.Reflection;

namespace OodHelper.Data
{
    /// <summary>
    /// Owns the LocalDB file locations and connection strings for the application database.
    /// Extracted from the legacy <c>Db</c> helper so EF Core registration (and any future consumer)
    /// does not depend on it — see App.xaml.cs's <c>AddDbContextFactory</c>.
    /// </summary>
    internal static class LocalDbConfig
    {
        public const string DatabaseName = "OodHelper";

        /// <summary>Connection to the LocalDB <c>master</c> catalog, for create/backup/single-user admin.</summary>
        public const string MasterConnectionString =
            @"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True";

        /// <summary>Folder under LocalApplicationData holding the attached .mdf.</summary>
        public static string DatabaseFolder { get; }

        /// <summary>Full path to the application database .mdf.</summary>
        public static string DataFileName { get; }

        /// <summary>Connection to the application database; used by EF Core and <see cref="DatabaseAdmin"/>.</summary>
        public static string ConnectionString { get; }

        static LocalDbConfig()
        {
            AssemblyName an = Assembly.GetAssembly(typeof(App)).GetName();
            DatabaseFolder =
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{an.Name}\data";
            DataFileName = $@"{DatabaseFolder}\{DatabaseName}.mdf";
            ConnectionString =
                $@"Data Source=(LocalDB)\v11.0;Initial Catalog={DatabaseName};Integrated Security=True;";
        }
    }
}
