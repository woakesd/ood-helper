using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.Properties;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configure : Window
    {
        public Configure()
        {
            InitializeComponent();

            //
            // init seed values
            //
            BottomSeed.Text = Settings.BottomSeed.ToString();
            TopSeed.Text= Settings.TopSeed.ToString();

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
            MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder();
            mcsb.Server = Server.Text;
            mcsb.UserID = Username.Text;
            mcsb.Password = Password.Text;
            mcsb.Database = Database.Text;
            mcsb.UseCompression = UseCompression.IsChecked.Value;
            uint port = mcsb.Port;
            UInt32.TryParse(Port.Text, out port);
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

            if (Int32.TryParse(BottomSeed.Text, out b) && b != 0)
                Settings.BottomSeed = b;

            if (Int32.TryParse(TopSeed.Text, out t) && t != 0)
                Settings.TopSeed = t;

            Db.ReseedDatabase();

            Settings.DefaultDiscardProfile = DefaultDiscardProfile.Text;

            DialogResult = true;
        }
    }
}
