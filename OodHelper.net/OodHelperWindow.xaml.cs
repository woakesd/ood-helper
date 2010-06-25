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
using System.Xml;
using System.Windows.Markup;

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

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            Working p = new Working(this, "Loading Boats", false, 0, 6);
            Common.copyMySqlData(p);
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SqlCe_Click(object sender, RoutedEventArgs e)
        {
            Db.CreateDb();
        }

        private void Boats_Click(object sender, RoutedEventArgs e)
        {
            /*XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = new string(' ', 4);
            settings.NewLineOnAttributes = true;
            StringBuilder strbuild = new StringBuilder();
            XmlWriter xmlwrite = XmlWriter.Create(strbuild, settings);
            ControlTemplate ct = Download.Template;
            XamlWriter.Save(Download.Template, xmlwrite);*/
            Boats b = new Boats();
            b.ShowDialog();
            b.HorizontalAlignment = HorizontalAlignment.Stretch;
            b.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void Calendar_Click(object sender, RoutedEventArgs e)
        {
            Races b = new Races();
            b.ShowDialog();
            b.HorizontalAlignment = HorizontalAlignment.Stretch;
            b.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Db.Compact();
        }

        private void People_Click(object sender, RoutedEventArgs e)
        {
            People p = new People();
            p.ShowDialog();
            p.HorizontalAlignment = HorizontalAlignment.Stretch;
            p.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About a = new About();
            a.ShowDialog();
        }

        private void importPY_Click(object sender, RoutedEventArgs e)
        {
            PNImport pni = new PNImport();
            pni.ShowDialog();
        }

        private void CreateHandicapDB_Click(object sender, RoutedEventArgs e)
        {
            HandicapDb.CreateDb();
        }

        private void SeriesResults_Click(object sender, RoutedEventArgs e)
        {
            SeriesChooser chooser = new SeriesChooser();
            if (chooser.ShowDialog().Value)
            {
                RaceSeriesResult rs = new RaceSeriesResult(chooser.Sid);
                SeriesDisplayByClass sd = new SeriesDisplayByClass(rs);
                dock.Children.Add(sd);
                sd.HorizontalAlignment = HorizontalAlignment.Stretch;
                sd.VerticalAlignment = VerticalAlignment.Stretch;
            }
        }

        private void Configuration_Click(object sender, RoutedEventArgs e)
        {
            Configuration f = new Configuration();
            f.ShowDialog();
        }
    }
}
