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
        private const string settDefaultDiscardProfile = "DefaultDiscardProfile";
        private const string settMysql = "mysql";
        private const string settBottomSeed = "bottomseed";
        private const string settTopSeed = "topseed";
        public const string ResultsWebServiceBaseURL = "ResultsWebServiceBaseURL";
        public const string ResultsWebServiceBaseUsername = "ResultsWebServiceBaseUsername";
        public const string ResultsWebServiceBasePassword = "ResultsWebServiceBasePassword";
        //public const string

        private static string CustomSettings = "./OodHelper.net.custom.config";
        private static Configuration Config;

        private static void CreateSettingsDb()
        {
            ExeConfigurationFileMap _myMap = new ExeConfigurationFileMap();
            _myMap.ExeConfigFilename = CustomSettings;
            Config = ConfigurationManager.OpenMappedExeConfiguration(_myMap, ConfigurationUserLevel.None);
        }

        private const int _bottomSeedDefault = 1000;

        public static int BottomSeed
        {
            get
            {
                int _tmp = 0;
                if (Int32.TryParse(GetSetting(settBottomSeed), out _tmp))
                    return _tmp;
                return _bottomSeedDefault;
            }
            set
            {
                AddSetting(settBottomSeed, value.ToString());
            }
        }

        private const int _topSeedDefault = 1999;

        public static int TopSeed
        {
            get
            {
                int _tmp = 0;
                if (Int32.TryParse(GetSetting(settTopSeed), out _tmp))
                    return _tmp;
                return _topSeedDefault;
            }
            set
            {
                AddSetting(settTopSeed, value.ToString());
            }
        }

        public static string Mysql
        {
            get
            {
                return GetSetting(settMysql);
            }

            set
            {
                AddSetting(settMysql, value);
            }
        }

        public static string DefaultDiscardProfile
        {
            get
            {
                return GetSetting(settDefaultDiscardProfile);
            }

            set
            {
                AddSetting(settDefaultDiscardProfile, value);
            }
        }

        private static void DeleteSetting(string name)
        {
            CreateSettingsDb();
            Config.AppSettings.Settings.Remove(name);
        }

        private static void AddSetting(string name, string value)
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

        private static string GetSetting(string name)
        {
            CreateSettingsDb();
            if (Config.AppSettings.Settings[name] != null)
                return Config.AppSettings.Settings[name].Value;
            return string.Empty;
        }
    }
}
