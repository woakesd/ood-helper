using System;
using System.Collections;
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
    /// Interaction logic for Seed.xaml
    /// </summary>
    public partial class Seed : Window
    {
        public Seed()
        {
            InitializeComponent();
            Object o;
            if ((o = DbSettings.GetSetting("bottomseed")) != null)
                BottomSeed.Text = o.ToString();
            else
                BottomSeed.Text = "1";
            if ((o = DbSettings.GetSetting("topseed")) != null)
                TopSeed.Text = o.ToString();
            else
                TopSeed.Text = "1999";
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            int b = 0, t = 0;

            if (Int32.TryParse(BottomSeed.Text, out b) && b != 0)
                DbSettings.AddSetting("bottomseed", b);
            else
                DbSettings.DeleteSetting("bottomseed");

            if (Int32.TryParse(TopSeed.Text, out t) && t != 0)
                DbSettings.AddSetting("topseed", t);
            else
                DbSettings.DeleteSetting("topseed");

            Db.ReseedDatabase();
            this.DialogResult = true;
            this.Close();
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            BottomSeed.Text = "";
            TopSeed.Text = "";
        }
    }
}
