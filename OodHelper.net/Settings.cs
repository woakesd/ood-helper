using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace OodHelper
{
    static public class Settings
    {
        private static readonly string _customSettings;
        private static readonly OodHelperSettings _oodHelperSettings;

        class OodHelperSettings
        {
            public string? mysql { get; set; }
            public int bottomseed { get; set; } = 1000;
            public int topseed { get; set; } = 1999;
            public double rollingHandicapCoefficient { get; set; } = 0.1;
            public string? defaultDiscardProfile { get; set; } = "0,1";
        }

        static Settings()
        {
            Assembly _ass = Assembly.GetExecutingAssembly();
            AssemblyName _an = _ass.GetName();
            CreateSettingsDb();
            _customSettings = string.Format($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{_an.Name}");
            _oodHelperSettings = new ConfigurationBuilder()
                .SetBasePath(_customSettings)
                .AddJsonFile("appSettings.json", true, true)
                .Build().Get<OodHelperSettings>();
            if (_oodHelperSettings == null)
                _oodHelperSettings = new OodHelperSettings();
        }

        private static void CreateSettingsDb()
        {
            //Config = ConfigurationManager.OpenMappedExeConfiguration(_myMap, ConfigurationUserLevel.None);
        }

        public static int BottomSeed
        {
            get
            {
                return _oodHelperSettings.bottomseed;
            }
            set
            {
                _oodHelperSettings.bottomseed = value;
            }
        }

        public static int TopSeed
        {
            get
            {
                return _oodHelperSettings.topseed;
            }
            set
            {
                _oodHelperSettings.topseed = value;
            }
        }

        private const double _rhCoefficientDefault = 0.1;

        public static double RHCoefficieent
        {
            get
            {
                return _oodHelperSettings.rollingHandicapCoefficient;
            }
            set
            {
                _oodHelperSettings.rollingHandicapCoefficient = value;
            }
        }

        public static string? Mysql
        {
            get
            {
                return _oodHelperSettings.mysql;
            }

            set
            {
                _oodHelperSettings.mysql = value;
            }
        }

        public static string? DefaultDiscardProfile
        {
            get
            {
                return _oodHelperSettings.defaultDiscardProfile;
            }

            set
            {
                _oodHelperSettings.defaultDiscardProfile = value;
            }
        }

        private static void SaveSettingsDb()
        {
            //Config.Save(ConfigurationSaveMode.Minimal, true);
        }
    }
}
