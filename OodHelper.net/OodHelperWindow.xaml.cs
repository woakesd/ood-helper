using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class OodHelperWindow : Window
    {
        public OodHelperWindow()
        {
            InitializeComponent();
            int i = Svn.Revision();
        }

        private void Results_Click(object sender, RoutedEventArgs e)
        {
            RaceChooser rc = new RaceChooser();
            rc.ShowDialog();
            int[] rids = rc.Rids;
            if (rids != null)
            {
                RaceResults r = new RaceResults(rids);
                dock.Children.Add(r);
                r.HorizontalAlignment = HorizontalAlignment.Stretch;
                r.VerticalAlignment = VerticalAlignment.Stretch;
            }
        }

        private void Admin_Click(object sender, RoutedEventArgs e)
        {
        }

        private void MySql_Click(object sender, RoutedEventArgs e)
        {
            Common.copyMySqlData();
        }

        private void SqlCe_Click(object sender, RoutedEventArgs e)
        {
            Db.CreateDb();
        }

        private void Boats_Click(object sender, RoutedEventArgs e)
        {
            Boats b = new Boats();
            b.ShowDialog();
            b.HorizontalAlignment = HorizontalAlignment.Stretch;
            b.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Db.Compact();
        }

        private void MySqlConfig_Click(object sender, RoutedEventArgs e)
        {
            MySqlForm f = new MySqlForm();
            f.ShowDialog();
        }

        private void People_Click(object sender, RoutedEventArgs e)
        {
            People p = new People();
            p.ShowDialog();
            p.HorizontalAlignment = HorizontalAlignment.Stretch;
            p.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void Seed_Click(object sender, RoutedEventArgs e)
        {
            Seed s = new Seed();
            s.ShowDialog();
        }
    }
}
