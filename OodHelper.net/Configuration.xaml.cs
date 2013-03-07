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
            string o;
            if ((o = Settings.GetSetting(Settings.settBottomSeed)) != string.Empty)
                BottomSeed.Text = o;
            else
                BottomSeed.Text = "1";
            if ((o = Settings.GetSetting(Settings.settTopSeed)) != null)
                TopSeed.Text = o;
            else
                TopSeed.Text = "1999";

            //
            // init mysql connection values
            //
            string myconstring = Settings.GetSetting(Settings.settMysql);
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
            DefaultDiscardProfile.Text = Settings.GetSetting(Settings.settDefaultDiscardProfile);
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
            Settings.AddSetting(Settings.settMysql, mcsb.ConnectionString);

            int b = 0, t = 0;

            if (Int32.TryParse(BottomSeed.Text, out b) && b != 0)
                Settings.AddSetting(Settings.settBottomSeed, BottomSeed.Text);
            else
                Settings.DeleteSetting(Settings.settBottomSeed);

            if (Int32.TryParse(TopSeed.Text, out t) && t != 0)
                Settings.AddSetting(Settings.settTopSeed, TopSeed.Text);
            else
                Settings.DeleteSetting(Settings.settTopSeed);

            Db.ReseedDatabase();

            Settings.AddSetting(Settings.settDefaultDiscardProfile, DefaultDiscardProfile.Text);

            DialogResult = true;
        }
    }
}
