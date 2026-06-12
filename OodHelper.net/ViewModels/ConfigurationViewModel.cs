using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MySqlConnector;
using OodHelper.Services;

namespace OodHelper.ViewModels
{
    public partial class ConfigurationViewModel : ObservableObject
    {
        // Indexes match the order of the SSL ComboBox items in Configuration.xaml.
        public const int SslNone = 0;
        public const int SslPreferred = 1;
        public const int SslRequired = 2;
        public const int SslVerifyCA = 3;
        public const int SslVerifyFull = 4;

        private readonly ISettingsService _settings;
        private readonly IDatabaseMaintenanceService _dbMaintenance;

        public event Action<bool> CloseRequested;

        [ObservableProperty]
        private string _bottomSeed;

        [ObservableProperty]
        private string _topSeed;

        [ObservableProperty]
        private string _rhCoefficient;

        [ObservableProperty]
        private string _rsCoefficient;

        [ObservableProperty]
        private string _server;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _database;

        [ObservableProperty]
        private string _port;

        [ObservableProperty]
        private bool _useCompression;

        [ObservableProperty]
        private int _sslModeIndex;

        [ObservableProperty]
        private string _defaultDiscardProfile;

        [ObservableProperty]
        private string _pusherAppId;

        [ObservableProperty]
        private string _pusherAppKey;

        [ObservableProperty]
        private string _pusherAppSecret;

        public ConfigurationViewModel(ISettingsService settings, IDatabaseMaintenanceService dbMaintenance)
        {
            _settings = settings;
            _dbMaintenance = dbMaintenance;

            BottomSeed = settings.BottomSeed.ToString();
            TopSeed = settings.TopSeed.ToString();
            RhCoefficient = settings.RhCoefficient.ToString();
            RsCoefficient = settings.RsCoefficient.ToString();

            var mcsb = new MySqlConnectionStringBuilder(settings.Mysql);
            Server = mcsb.Server;
            Username = mcsb.UserID;
            Password = mcsb.Password;
            Database = mcsb.Database;
            Port = mcsb.Port.ToString();
            UseCompression = mcsb.UseCompression;
            SslModeIndex = SslNone;
            if (mcsb.SslMode.HasFlag(MySqlSslMode.Preferred))
                SslModeIndex = SslPreferred;
            else if (mcsb.SslMode.HasFlag(MySqlSslMode.Required))
                SslModeIndex = SslRequired;
            if (mcsb.SslMode.HasFlag(MySqlSslMode.VerifyCA))
                SslModeIndex = SslVerifyCA;
            else if (mcsb.SslMode.HasFlag(MySqlSslMode.VerifyFull))
                SslModeIndex = SslVerifyFull;

            PusherAppId = settings.PusherAppId;
            PusherAppKey = settings.PusherAppKey;
            PusherAppSecret = settings.PusherAppSecret;

            DefaultDiscardProfile = settings.DefaultDiscardProfile;
            if (DefaultDiscardProfile == string.Empty)
                DefaultDiscardProfile = "0,1";
        }

        [RelayCommand]
        private void ClearSeeds()
        {
            BottomSeed = string.Empty;
            TopSeed = string.Empty;
        }

        [RelayCommand]
        private void Save()
        {
            var mcsb = new MySqlConnectionStringBuilder
            {
                Server = Server,
                UserID = Username,
                Password = Password,
                Database = Database,
                UseCompression = UseCompression
            };

            if (uint.TryParse(Port, out uint port))
                mcsb.Port = port;
            if (SslModeIndex == SslPreferred)
                mcsb.SslMode = MySqlSslMode.Preferred;
            if (SslModeIndex == SslRequired)
                mcsb.SslMode = MySqlSslMode.Required;
            if (SslModeIndex == SslVerifyCA)
                mcsb.SslMode = MySqlSslMode.VerifyCA;
            if (SslModeIndex == SslVerifyFull)
                mcsb.SslMode = MySqlSslMode.VerifyFull;
            _settings.Mysql = mcsb.ConnectionString;

            if (int.TryParse(BottomSeed, out int b) && b != 0)
                _settings.BottomSeed = b;

            if (int.TryParse(TopSeed, out int t) && t != 0)
                _settings.TopSeed = t;

            _dbMaintenance.Reseed();

            if (double.TryParse(RhCoefficient, out double rhc))
                _settings.RhCoefficient = rhc;

            if (double.TryParse(RsCoefficient, out double rsc))
                _settings.RsCoefficient = rsc;

            _settings.DefaultDiscardProfile = DefaultDiscardProfile;

            _settings.PusherAppId = PusherAppId;
            _settings.PusherAppKey = PusherAppKey;
            _settings.PusherAppSecret = PusherAppSecret;

            CloseRequested?.Invoke(true);
        }
    }
}
