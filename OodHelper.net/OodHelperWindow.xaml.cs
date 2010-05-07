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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    [Svn("$Id: OodHelperWindow.xaml.cs 17589 2010-05-04 21:35:53Z david $")]
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
                //r.ShowDialog();
                dock.Children.Add(r);
                r.HorizontalAlignment = HorizontalAlignment.Stretch;
                r.VerticalAlignment = VerticalAlignment.Stretch;
                //dock.Children[0].
            }
        }

        private void Admin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MySql_Click(object sender, RoutedEventArgs e)
        {
            Common.copyMySqlData();
        }

        private void Boats_Click(object sender, RoutedEventArgs e)
        {
            Boats b = new Boats();
            b.ShowDialog();
            //dock.Children.Add(b);
            b.HorizontalAlignment = HorizontalAlignment.Stretch;
            b.VerticalAlignment = VerticalAlignment.Stretch;

        }
    }
}
