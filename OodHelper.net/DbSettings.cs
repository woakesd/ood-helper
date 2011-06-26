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

            if (!File.Exists(@".\data\settings.sdf"))
            {
                SqlCeEngine ce = new SqlCeEngine(constr);
                ce.CreateDatabase();
                ce.Dispose();

                SqlCeConnection con = new SqlCeConnection(constr);
                con.Open();
                SqlCeCommand cmd = new SqlCeCommand();
                cmd.Connection = con;
                cmd.CommandText = "CREATE TABLE [settings] (" +
                    "[id] int NOT NULL IDENTITY(1,1) " +
                    ", [name] nvarchar(50) NOT NULL" +
                    ", [value] image NOT NULL)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"ALTER TABLE [settings] ADD CONSTRAINT [PK_settings] PRIMARY KEY ([id])";
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        public static void AddSetting(string name, object value)
        {
            CreateSettingsDb();
            MemoryStream ms = new MemoryStream();
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter form = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            form.Serialize(ms, value);
            byte[] bytes = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(bytes, 0, (int) ms.Length);
            SqlCeConnection con = new SqlCeConnection(Properties.Settings.Default.SettingsConnectionString);
            try
            {
                con.Open();
                SqlCeCommand cmd = new SqlCeCommand();
                cmd.Connection = con;
                cmd.CommandText = "UPDATE settings " +
                    "SET value = @value " +
                    "WHERE name = @name";
                cmd.Parameters.Add(new SqlCeParameter("name", name));
                SqlCeParameter p = new SqlCeParameter("value", SqlDbType.Image, bytes.Length);
                p.Value = bytes;
                cmd.Parameters.Add(p);
                if (cmd.ExecuteNonQuery() == 0)
                {
                    cmd.CommandText = "INSERT INTO settings " +
                        "(name, value) " +
                        "VALUES (@name, @value)";
                    int cnt = cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                con.Close();
            }
        }

        public static void DeleteSetting(string name)
        {
            CreateSettingsDb();
            SqlCeConnection con = new SqlCeConnection(Properties.Settings.Default.SettingsConnectionString);
            try
            {
                con.Open();
                SqlCeCommand cmd = new SqlCeCommand();
                cmd.Connection = con;
                cmd.CommandText = "DELETE FROM settings " +
                    "WHERE name = @name";
                cmd.Parameters.Add(new SqlCeParameter("name", name));
                cmd.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
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
