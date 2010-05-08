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

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for MySqlForm.xaml
    /// </summary>
    public partial class MySqlForm : Window
    {
        public MySqlForm()
        {
            InitializeComponent();
            string myconstring = (string)DbSettings.GetSetting("mysql");
            MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(myconstring);
            Server.Text = mcsb.Server;
            Username.Text = mcsb.UserID;
            Password.Text = mcsb.Password;
            Database.Text = mcsb.Database;
            UseCompression.IsChecked = mcsb.UseCompression;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder();
            mcsb.Server = Server.Text;
            mcsb.UserID = Username.Text;
            mcsb.Password = Password.Text;
            mcsb.Database = Database.Text;
            mcsb.UseCompression = UseCompression.IsChecked.Value;
            DbSettings.AddSetting("mysql", mcsb.ConnectionString);
            DialogResult = true;
        }
    }
}
