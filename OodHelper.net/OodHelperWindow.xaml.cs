using System;
using System.ComponentModel;
using System.Data;
using System.Printing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private void Results_Click(object sender, RoutedEventArgs e)
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
                new UploadResults();
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

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var a = new About();
            a.ShowDialog();
        }

        private void importPY_Click(object sender, RoutedEventArgs e)
        {
            var pni = new PNImport();
            pni.ShowDialog();
        }

        private void SeriesResults_Click(object sender, RoutedEventArgs e)
        {
            var chooser = new SeriesChooser();
            var val = chooser.ShowDialog();
            if (!val.HasValue || !val.Value) return;

            RaceSeriesResult rs = null;
            myDelegate uiDelegate = delegate
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

        private void Foxpro_Click(object sender, RoutedEventArgs e)
        {
            var fx = new FoxproImport();
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
                    var dtask = new DownloadResults();
                }
            }
        }

        private void ExportResults_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PrintMembershipCards_Click(object sender, RoutedEventArgs e)
        {
            var pd = new PrintDialog();
            //pd.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(1718.4, 1228.8);
            //pd.PrintTicket.PageOrientation = PageOrientation.Landscape;

            if (pd.ShowDialog() == true)
            {
                var d = new Db(@"SELECT id, main_id, firstname, surname, firstname as order2, surname as order1, member
                    FROM people
                    WHERE cp = 1
                    AND main_id = id
                    UNION
                    SELECT p2.id, p2.main_id, p2.firstname, p2.surname, p1.firstname as order2, p1.surname as order1, p2.member
                    FROM people p1 INNER JOIN people p2 ON p1.id = p2.main_id
                    WHERE p1.cp = 1
                    AND p2.main_id <> p2.id
                    ORDER BY order1, order2");
                DataTable data = d.GetData(null);

                var w = new Working(this);
                var pages = (int) Math.Floor(data.Rows.Count/10.0);
                if (data.Rows.Count > pages*10) pages++;
                w.SetRange(0, pages + 1);
                w.Show();
                var ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                XpsDocumentWriter write = PrintQueue.CreateXpsDocumentWriter(pd.PrintQueue);
                var collator = write.CreateVisualsCollator() as VisualsToXpsDocument;

                Task t = Task.Factory.StartNew(() =>
                {
                    Dispatcher.Invoke(delegate { collator.BeginBatchWrite(); });

                    w.SetProgress("Printing ", 0);
                    Thread.Sleep(50);

                    CardPage cp = null;
                    Dispatcher.Invoke(delegate { cp = new CardPage(); });

                    int grow = 0;
                    int gcol = 0;
                    int index = 0;

                    for (index = 0; index < data.Rows.Count; index++)
                    {
                        grow = index%5;
                        gcol = (index%10 - grow)/5;

                        if (index > 0 && grow == 0 && gcol == 0)
                        {
                            w.SetProgress("Printing ", index/10);
                            Thread.Sleep(50);
                            Dispatcher.Invoke(delegate
                            {
                                var pageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                                cp.Measure(pageSize);
                                cp.Arrange(new Rect(new Point(0, 0), pageSize));
                                cp.UpdateLayout();
                                collator.Write(cp, pd.PrintTicket);
                                cp = new CardPage();
                            });
                        }

                        Dispatcher.Invoke(delegate
                        {
                            DataRow dr = data.Rows[index];
                            var c = new Card(string.Format("{0} {1}", new[] {dr["firstname"], dr["surname"]}),
                                (int) dr["id"], (int) dr["main_id"], dr["member"] as string);
                            var cardSize = new Size(324, 204);
                            c.Measure(cardSize);
                            c.Arrange(new Rect(new Point(0, 0), cardSize));
                            c.UpdateLayout();
                            cp.Cards.Children.Add(c);
                            Grid.SetColumn(c, gcol);
                            Grid.SetRow(c, grow);
                        });
                    }

                    if (index > 0)
                    {
                        Dispatcher.Invoke(delegate
                        {
                            var pageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                            cp.Measure(pageSize);
                            cp.Arrange(new Rect(new Point(0, 0), pageSize));
                            cp.UpdateLayout();
                            collator.Write(cp, pd.PrintTicket);
                        });
                    }

                    Dispatcher.Invoke(delegate
                    {
                        cp = new CardPage();
                        for (grow = 0; grow < 5; grow++)
                        {
                            for (gcol = 0; gcol < 2; gcol++)
                            {
                                var c = new Card();
                                var cardSize = new Size(324, 204);
                                c.Measure(cardSize);
                                c.Arrange(new Rect(new Point(0, 0), cardSize));
                                c.UpdateLayout();
                                cp.Cards.Children.Add(c);
                                Grid.SetColumn(c, gcol);
                                Grid.SetRow(c, grow);
                            }
                        }

                        var pageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                        cp.Measure(pageSize);
                        cp.Arrange(new Rect(new Point(0, 0), pageSize));
                        cp.UpdateLayout();
                        collator.Write(cp, pd.PrintTicket);
                        collator.Write(cp, pd.PrintTicket);
                        collator.EndBatchWrite();
                    });

                    w.CloseWindow();
                });
            }
        }

        private class Data : NotifyPropertyChanged
        {
            private bool _ShowPrivilegedItems;

            public bool ShowPrivilegedItems
            {
                get { return _ShowPrivilegedItems; }
                set
                {
                    _ShowPrivilegedItems = value;
                    OnPropertyChanged("ShowPrivilegedItems");
                    OnPropertyChanged("HideNonPrivilegedItems");
                }
            }

            public bool HideNonPrivilegedItems
            {
                get { return !_ShowPrivilegedItems; }
            }
        }

        private delegate void myDelegate();
    }
}