using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;
using System.Runtime.Serialization.Formatters;

namespace OodHelper
{
    static public class DbSettings
    {
        public const string settDefaultDiscardProfile = "DefaultDiscardProfile";
        public const string settMysql = "mysql";
        public const string settBottomSeed = "bottomseed";
        public const string settTopSeed = "topseed";

        private static void CreateSettingsDb()
        {
            string constr = Properties.Settings.Default.SettingsConnectionString;

            if (!Directory.Exists(@".\data"))
                Directory.CreateDirectory(@".\data");

            //
            // NB Settings DB is being removed and we are using a file instead to this doesn't create
            // a db, but will upgrade an existing one if it is present.
            //

            if (File.Exists(@".\data\settings.sdf"))
            {
                //
                // Check if we need to upgrade the db
                //
                using (SqlCeConnection con = new SqlCeConnection(Properties.Settings.Default.SettingsConnectionString))
                {
                    try
                    {
                        con.Open();
                    }
                    catch
                    {
                        SqlCeEngine _eng = new SqlCeEngine(Properties.Settings.Default.SettingsConnectionString);
                        _eng.Upgrade();
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
        }

        public static object GetSetting(string name)
        {
            CreateSettingsDb();
            SqlCeConnection con = new SqlCeConnection(Properties.Settings.Default.SettingsConnectionString);
            try
            {
                con.Open();
                SqlCeCommand cmd = new SqlCeCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT value FROM settings " +
                    "WHERE name = @name";
                cmd.Parameters.Add(new SqlCeParameter("name", name));
                byte[] data = (byte[]) cmd.ExecuteScalar();
                if (data != null)
                {
                    MemoryStream ms = new MemoryStream(data);
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter form = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    return form.Deserialize(ms);
                }
                else
                    return null;
            }
            finally
            {
                con.Close();
            }
        }
    }
}
