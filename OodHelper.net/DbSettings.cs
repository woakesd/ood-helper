using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;
using System.Runtime.Serialization.Formatters;

namespace OodHelper
{
    static public class Settings
    {
        public const string settDefaultDiscardProfile = "DefaultDiscardProfile";
        public const string settMysql = "mysql";
        public const string settBottomSeed = "bottomseed";
        public const string settTopSeed = "topseed";
        //public const string

        private static string CustomSettings = "./OodHelper.net.custom.config";
        private static Configuration Config;

        private static void CreateSettingsDb()
        {
            ExeConfigurationFileMap _myMap = new ExeConfigurationFileMap();
            _myMap.ExeConfigFilename = CustomSettings;
            Config = ConfigurationManager.OpenMappedExeConfiguration(_myMap, ConfigurationUserLevel.None);
        }

        public static void DeleteSetting(string name)
        {
            CreateSettingsDb();
            Config.AppSettings.Settings.Remove(name);
        }

        public static void AddSetting(string name, string value)
        {
            CreateSettingsDb();
            bool _saveNeeded = false;

            if (Config.AppSettings.Settings[name] == null)
            {
                Config.AppSettings.Settings.Add(name, value);
                _saveNeeded = true;
            }
            else if (Config.AppSettings.Settings[name].Value != value)
            {
                Config.AppSettings.Settings[name].Value = value;
                _saveNeeded = true;
            }

            if (_saveNeeded)
                SaveSettingsDb();
        }

        private static void SaveSettingsDb()
        {
            Config.Save(ConfigurationSaveMode.Minimal, true);
        }

        public static string GetSetting(string name)
        {
            CreateSettingsDb();
            if (Config.AppSettings.Settings[name] != null)
                return Config.AppSettings.Settings[name].Value;
            return string.Empty;
        }
    }
}
