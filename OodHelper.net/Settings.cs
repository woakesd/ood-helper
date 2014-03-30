using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Data;
using System.Runtime.Serialization.Formatters;
using System.Reflection;

namespace OodHelper
{
    static public class Settings
    {
        private const string settDefaultDiscardProfile = "DefaultDiscardProfile";
        private const string settMysql = "mysql";
        private const string settBottomSeed = "bottomseed";
        private const string settTopSeed = "topseed";

        private const string settResultsWebServiceBaseURL = "ResultsWebServiceBaseURL";
        private const string settResultsWebServiceBaseUsername = "ResultsWebServiceBaseUsername";
        private const string settResultsWebServiceBasePassword = "ResultsWebServiceBasePassword";

        private const string settPusherAppId = "PusherAppId";
        private const string settPusherAppKey = "PusherAppKey";
        private const string settPusherAppSecret = "PusherAppSecret";

        private static string CustomSettings;
        private static Configuration Config;

        static Settings()
        {
            Assembly _ass = Assembly.GetExecutingAssembly();
            AssemblyName _an = _ass.GetName();
            CustomSettings = string.Format(@"{0}\{1}\OodHelper.net.custom.config", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _an.Name);
        }

        public static string ResultsWebServiceBaseURL
        {
            get
            {
                return GetSetting(settResultsWebServiceBaseURL);
            }
            set
            {
                AddSetting(settResultsWebServiceBaseURL, value);
            }
        }

        public static string ResultsWebServiceBaseUsername
        {
            get
            {
                return GetSetting(settResultsWebServiceBaseUsername);
            }
            set
            {
                AddSetting(settResultsWebServiceBaseUsername, value);
            }
        }

        public static string ResultsWebServiceBasePassword
        {
            get
            {
                return GetSetting(settResultsWebServiceBasePassword);
            }
            set
            {
                AddSetting(settResultsWebServiceBasePassword, value);
            }
        }

        public static string PusherAppId
        {
            get
            {
                return GetSetting(settPusherAppId);
            }
            set
            {
                AddSetting(settPusherAppId, value);
            }
        }

        public static string PusherAppKey
        {
            get
            {
                return GetSetting(settPusherAppKey);
            }
            set
            {
                AddSetting(settPusherAppKey, value);
            }
        }

        public static string PusherAppSecret
        {
            get
            {
                return GetSetting(settPusherAppSecret);
            }
            set
            {
                AddSetting(settPusherAppSecret, value);
            }
        }

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
