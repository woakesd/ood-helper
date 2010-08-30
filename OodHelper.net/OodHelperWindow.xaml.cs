using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
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
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class OodHelperWindow : Window, INotifyPropertyChanged
    {
        public OodHelperWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        bool _ShowPrivilegedItems = false;
        public bool ShowPrivilegedItems
        {
            get
            {
                return _ShowPrivilegedItems;
            }
            set
            {
                _ShowPrivilegedItems = value;
                OnPropertyChanged("ShowPrivilegedItems");
                OnPropertyChanged("HideNonPrivilegedItems");
            }
        }

        public bool HideNonPrivilegedItems
        {
            get
            {
                return !_ShowPrivilegedItems;
            }
        }

        private void Results_Click(object sender, RoutedEventArgs e)
        {
            RaceChooser rc = new RaceChooser();
            if (rc.ShowDialog().Value)
            {
                int[] rids = rc.Rids;
                if (rids != null)
                {
                    RaceResults r = new RaceResults(rids);
                    dock.Children.Add(r);
                    r.HorizontalAlignment = HorizontalAlignment.Stretch;
                    r.VerticalAlignment = VerticalAlignment.Stretch;
                }
            }
        }

        private void Admin_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            Working p = new Working(this, "Downloading Boats", false, 0, 6);
            Common.copyMySqlData(p);
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            Working p = new Working(this, "Uploading Boats", false, 0, 6);
            Common.copyToMySql(p);
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
            Maintain.Boats b = new Maintain.Boats();
            b.ShowDialog();
            b.HorizontalAlignment = HorizontalAlignment.Stretch;
            b.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void Series_Click(object sender, RoutedEventArgs e)
        {
            Maintain.Series b = new Maintain.Series();
            b.ShowDialog();
        }

        private void Calendar_Click(object sender, RoutedEventArgs e)
        {
            Races b = new Races();
            b.ShowDialog();
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

        private delegate void myDelegate();
        
        private void SeriesResults_Click(object sender, RoutedEventArgs e)
        {
            SeriesChooser chooser = new SeriesChooser();
            if (chooser.ShowDialog().Value)
            {
                RaceSeriesResult rs = null;
                myDelegate d = delegate()
                {
                    SeriesDisplayByClass sd = new SeriesDisplayByClass(rs);
                    dock.Children.Add(sd);
                    sd.HorizontalAlignment = HorizontalAlignment.Stretch;
                    sd.VerticalAlignment = VerticalAlignment.Stretch;
                };
                rs = new RaceSeriesResult(chooser.Sid, d);
            }
        }

        private void Configuration_Click(object sender, RoutedEventArgs e)
        {
            Configuration f = new Configuration();
            f.ShowDialog();
        }

        private void Handicaps_Click(object sender, RoutedEventArgs e)
        {
            Handicaps h = new Handicaps();
            h.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            ShowPrivilegedItems = true; // Visibility.Visible;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            ShowPrivilegedItems = false; // Visibility.Collapsed;
        }

        private void EntrySheets_Click(object sender, RoutedEventArgs e)
        {
            EntrySheetSelector sel = new EntrySheetSelector();
            sel.ShowDialog();
        }

        private void Foxpro_Click(object sender, RoutedEventArgs e)
        {
            FoxproImport fx = new FoxproImport();

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            (new Rules.SetupRules()).ShowDialog();
        }
    }
}
