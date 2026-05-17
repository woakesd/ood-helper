using System;
using System.Windows;
using MySqlConnector;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configure : Window
    {
        public Configure()
        {
            Owner = App.Current.MainWindow;
            InitializeComponent();

            //
            // init seed values
            //
            BottomSeed.Text = Settings.BottomSeed.ToString();
            TopSeed.Text= Settings.TopSeed.ToString();

            //
            // Get rolling handicap coefficient
            //
            RHCoefficient.Text = Settings.RHCoefficieent.ToString();

            //
            // Get rolling handicap coefficient
            //
            RSCoefficient.Text = Settings.RSCoefficieent.ToString();

            //
            // init mysql connection values
            //
            string myconstring = Settings.Mysql;
            MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(myconstring);
            Server.Text = mcsb.Server;
            Username.Text = mcsb.UserID;
            Password.Text = mcsb.Password;
            Database.Text = mcsb.Database;
            Port.Text = mcsb.Port.ToString();
            UseCompression.IsChecked = mcsb.UseCompression;
            if (mcsb.SslMode.HasFlag(MySqlSslMode.Preferred))
                SSL.SelectedValue = SslPreferred;
            else if (mcsb.SslMode.HasFlag(MySqlSslMode.Required))
                SSL.SelectedValue = SslRequired;
            if (mcsb.SslMode.HasFlag(MySqlSslMode.VerifyCA))
                SSL.SelectedValue = SslVerifyCA;
            else if (mcsb.SslMode.HasFlag(MySqlSslMode.VerifyFull))
                SSL.SelectedValue = SslVerifyFull;

            PusherAppId.Text = Settings.PusherAppId;
            PusherAppKey.Text = Settings.PusherAppKey;
            PusherAppSecret.Text = Settings.PusherAppSecret;

            //
            // init default discard profile
            //
            DefaultDiscardProfile.Text = Settings.DefaultDiscardProfile;
            if (DefaultDiscardProfile.Text == string.Empty)
                DefaultDiscardProfile.Text = "0,1";
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            BottomSeed.Text = string.Empty;
            TopSeed.Text = string.Empty;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder
            {
                Server = Server.Text,
                UserID = Username.Text,
                Password = Password.Text,
                Database = Database.Text,
                UseCompression = UseCompression.IsChecked.Value
            };
            
            if (uint.TryParse(Port.Text, out uint port))
                mcsb.Port = port;
            if (SSL.SelectedValue == SslPreferred)
                mcsb.SslMode = MySqlSslMode.Preferred;
            if (SSL.SelectedValue == SslRequired)
                mcsb.SslMode = MySqlSslMode.Required;
            if (SSL.SelectedValue == SslVerifyCA)
                mcsb.SslMode = MySqlSslMode.VerifyCA;
            if (SSL.SelectedValue == SslVerifyFull)
                mcsb.SslMode = MySqlSslMode.VerifyFull;
            Settings.Mysql = mcsb.ConnectionString;

            int b = 0, t = 0;

            if (int.TryParse(BottomSeed.Text, out b) && b != 0)
                Settings.BottomSeed = b;

            if (int.TryParse(TopSeed.Text, out t) && t != 0)
                Settings.TopSeed = t;

            Db.ReseedDatabase();

            if (double.TryParse(RHCoefficient.Text, out double _rhc))
                Settings.RHCoefficieent = _rhc;

            if (double.TryParse(RSCoefficient.Text, out double _rsc))
                Settings.RSCoefficieent = _rsc;

            Settings.DefaultDiscardProfile = DefaultDiscardProfile.Text;

            Settings.PusherAppId = PusherAppId.Text;
            Settings.PusherAppKey = PusherAppKey.Text;
            Settings.PusherAppSecret = PusherAppSecret.Text;

            DialogResult = true;
        }
    }
}
