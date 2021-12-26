using System;
using System.ComponentModel;
using System.Data;
using System.Printing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Xps;
using OodHelper.LoadTide;
using OodHelper.Maintain;
using OodHelper.Membership;
using OodHelper.Results;
using OodHelper.Rules;
using OodHelper.Sun;
using OodHelper.Website;

namespace OodHelper
{
    /// <summary>
    ///     Interaction logic for Window1.xaml
    /// </summary>
    public partial class OodHelperWindow
    {
        private readonly Data _data = new Data();

        public OodHelperWindow()
        {
            InitializeComponent();
            DataContext = _data;
        }

        private void Results_Click(object sender, ExecutedRoutedEventArgs e)
        {
            var rc = new RaceChooser();
            var val = rc.ShowDialog();
            if (!val.HasValue || !val.Value) return;

            var rids = rc.Rids;
            if (rids == null) return;

            var r = new RaceResults(rids);
            var rp = new TabItem {Content = r, Header = "Race Results"};
            dock.Items.Add(rp);
            dock.SelectedItem = rp;
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Click OK to confirm downloading database from Website", "Confirm Download",
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new DownloadResults();
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Click OK to confirm uploading database to Website", "Confirm Upload",
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK)
            {
                // ReSharper disable once ObjectCreationAsStatement
                var worker = new UploadResults();
                worker.Run();
            }
        }

        private void SqlCe_Click(object sender, RoutedEventArgs e)
        {
            Db.CreateDb();
        }

        private void Boats_Click(object sender, RoutedEventArgs e)
        {
            var b = new Boats();
            b.ShowDialog();
            b.HorizontalAlignment = HorizontalAlignment.Stretch;
            b.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void Series_Click(object sender, RoutedEventArgs e)
        {
            var b = new Series();
            b.ShowDialog();
        }

        private void Calendar_Click(object sender, RoutedEventArgs e)
        {
            var b = new Races();
            b.ShowDialog();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }

        private void People_Click(object sender, RoutedEventArgs e)
        {
            var p = new PeopleList();
            p.ShowDialog();
            p.HorizontalAlignment = HorizontalAlignment.Stretch;
            p.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AboutOod_Click(object sender, RoutedEventArgs e)
        {
            var a = new AboutOod();
            a.ShowDialog();
        }

        private void importPY_Click(object sender, RoutedEventArgs e)
        {
            var pni = new PnImport();
            pni.ShowDialog();
        }

        private void SeriesResults_Click(object sender, RoutedEventArgs e)
        {
            var chooser = new SeriesChooser();
            var val = chooser.ShowDialog();
            if (!val.HasValue || !val.Value) return;

            RaceSeriesResult rs = null;
            MyDelegate uiDelegate = delegate
            {
                var resultDisplayByClass = new SeriesDisplayByClass(rs);
                var seriesDisplayTab = new TabItem {Content = resultDisplayByClass, Header = "Series Result"};
                dock.Items.Add(seriesDisplayTab);
                dock.SelectedItem = seriesDisplayTab;
            };
            rs = new RaceSeriesResult(chooser.Sid, uiDelegate);
        }

        private void Configuration_Click(object sender, RoutedEventArgs e)
        {
            var f = new Configure();
            f.ShowDialog();
        }

        private void Handicaps_Click(object sender, RoutedEventArgs e)
        {
            var h = new Handicaps();
            h.ShowDialog();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            _data.ShowPrivilegedItems = true;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _data.ShowPrivilegedItems = false;
        }

        private void EntrySheets_Click(object sender, RoutedEventArgs e)
        {
            var sel = new EntrySheetSelector();
            sel.ShowDialog();
        }

        private void Rule_Click(object sender, RoutedEventArgs e)
        {
            (new SelectRules()).ShowDialog();
        }

        private void Tide_Click(object sender, RoutedEventArgs e)
        {
            var rd = new ReadData();
            rd.ShowDialog();
        }

        private void Sun_Click(object sender, RoutedEventArgs e)
        {
            var sd = new DoSunSetRise();
            sd.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var c = new CheckForUpdates();
            if (c.LocalDate < c.RemoteDate)
            {
                if (MessageBox.Show("Website results are more up to date\nWould you like to download from Website",
                    "Confirm Download",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK)
                {
                    new DownloadResults();
                }
            }
        }

        private void ExportResults_Click(object sender, RoutedEventArgs e)
        {
        }

        private class Data : NotifyPropertyChanged
        {
            private bool _showPrivilegedItems;

            public bool ShowPrivilegedItems
            {
                get { return _showPrivilegedItems; }
                set
                {
                    _showPrivilegedItems = value;
                    OnPropertyChanged("ShowPrivilegedItems");
                    OnPropertyChanged("HideNonPrivilegedItems");
                }
            }
        }

        private delegate void MyDelegate();
    }
}