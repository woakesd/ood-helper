using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for Boats.xaml
    /// </summary>
    [Svn("$Id: OodHelperWindow.xaml.cs 17588 2010-05-04 19:37:30Z david $")]
    public partial class Boats : Window
    {
        public Boats()
        {
            InitializeComponent();
            Db c = new Db("SELECT * FROM boats");
            DataTable bts = c.GetData(null);
            BoatData.ItemsSource = bts.DefaultView;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddBoat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditBoat_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
