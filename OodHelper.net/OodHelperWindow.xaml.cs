using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OodHelper.Maintain;
using OodHelper.Website;
using OodHelper.Sun;
using System.Printing;
using System.Windows.Xps;
using OodHelper.Results;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class OodHelperWindow : Window
    {
        public OodHelperWindow()
        {
            InitializeComponent();
            DataContext = dc;
        }

        private class Data : NotifyPropertyChanged
        {
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
        }

        private Data dc = new Data();

        private void Results_Click(object sender, RoutedEventArgs e)
        {
            RaceChooser rc = new RaceChooser();
            if (rc.ShowDialog().Value)
            {
                int[] rids = rc.Rids;
                if (rids != null)
                {
                    Results.RaceResults r = new Results.RaceResults(rids);
                    TabItem rp = new TabItem();
                    rp.Content = r;
                    rp.Header = "Race Results";
                    dock.Items.Add(rp);
                    dock.SelectedItem = rp;
                }
            }
        }

        private void Admin_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Click OK to confirm downloading database from Website", "Confirm Download", 
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK)
            {
                DownloadResults dtask = new DownloadResults();
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Click OK to confirm uploading database to Website", "Confirm Upload",
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK)
            {
                UploadResults utask = new UploadResults();
            }
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

        private void Series_Click(object sender, RoutedEventArgs e)
        {
            Series b = new Series();
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

        private delegate void myDelegate();
        
        private void SeriesResults_Click(object sender, RoutedEventArgs e)
        {
            SeriesChooser _chooser = new SeriesChooser();
            if (_chooser.ShowDialog().Value)
            {
                RaceSeriesResult rs = null;
                myDelegate _uiDelegate = delegate()
                {
                    SeriesDisplayByClass _ResultDisplayByClass = new SeriesDisplayByClass(rs);
                    TabItem _seriesDisplayTab = new TabItem();
                    _seriesDisplayTab.Content = _ResultDisplayByClass;
                    _seriesDisplayTab.Header = "Series Result";
                    dock.Items.Add(_seriesDisplayTab);
                    dock.SelectedItem = _seriesDisplayTab;
                };
                rs = new RaceSeriesResult(_chooser.Sid, _uiDelegate);
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

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            dc.ShowPrivilegedItems = true; 
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            dc.ShowPrivilegedItems = false;
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

        private void Rule_Click(object sender, RoutedEventArgs e)
        {
            (new Rules.SelectRules()).ShowDialog();
        }

        private void Tide_Click(object sender, RoutedEventArgs e)
        {
            LoadTide.ReadData rd = new LoadTide.ReadData();
            rd.ShowDialog();
        }

        private void Sun_Click(object sender, RoutedEventArgs e)
        {
            DoSunSetRise sd = new DoSunSetRise();
            sd.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckForUpdates c = new CheckForUpdates();
            if (c.LocalDate < c.RemoteDate)
            {
                if (MessageBox.Show("Website results are more up to date\nWould you like to download from Website", "Confirm Download",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK)
                {
                    DownloadResults dtask = new DownloadResults();
                }
            }
        }

        private void ExportResults_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PrintMembershipCards_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            //pd.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(1718.4, 1228.8);
            //pd.PrintTicket.PageOrientation = PageOrientation.Landscape;

            if (pd.ShowDialog() == true)
            {
                Db d = new Db(@"SELECT id, main_id, firstname, surname, firstname as order2, surname as order1, member
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

                Working w = new Working();
                int pages = (int)Math.Floor(data.Rows.Count / 10.0);
                if (data.Rows.Count > pages * 10) pages++;
                w.SetRange(0, pages + 1);
                w.Show();
                Size ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                XpsDocumentWriter write = PrintQueue.CreateXpsDocumentWriter(pd.PrintQueue);
                VisualsToXpsDocument collator = write.CreateVisualsCollator() as VisualsToXpsDocument;

                System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Dispatcher.Invoke(new Action(delegate()
                    {
                        collator.BeginBatchWrite();
                    }));

                    w.SetProgress("Printing ", 0);
                    System.Threading.Thread.Sleep(50);

                    Membership.CardPage cp = null;
                    Dispatcher.Invoke(new Action(delegate()
                    {
                        cp = new Membership.CardPage();
                    }));

                    int grow = 0;
                    int gcol = 0;
                    int index = 0;

                    for (index = 0; index < data.Rows.Count; index++)
                    {
                        grow = index % 5;
                        gcol = (index % 10 - grow) / 5;

                        if (index > 0 && grow == 0 && gcol == 0)
                        {
                            w.SetProgress("Printing ", index / 10);
                            System.Threading.Thread.Sleep(50);
                            Dispatcher.Invoke(new Action(delegate()
                            {
                                Size pageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                                cp.Measure(pageSize);
                                cp.Arrange(new Rect(new Point(0, 0), pageSize));
                                cp.UpdateLayout();
                                collator.Write(cp, pd.PrintTicket);
                                cp = new Membership.CardPage();
                            }));
                        }

                        Dispatcher.Invoke(new Action(delegate()
                        {
                            DataRow dr = data.Rows[index];
                            Membership.Card c = new Membership.Card(string.Format("{0} {1}", new object[] { dr["firstname"], dr["surname"] }), (int)dr["id"], (int)dr["main_id"], dr["member"] as string);
                            Size cardSize = new Size(324, 204);
                            c.Measure(cardSize);
                            c.Arrange(new Rect(new Point(0, 0), cardSize));
                            c.UpdateLayout();
                            cp.Cards.Children.Add(c);
                            Grid.SetColumn(c, gcol);
                            Grid.SetRow(c, grow);
                        }));
                    }

                    if (index > 0)
                    {
                        Dispatcher.Invoke(new Action(delegate()
                        {
                            Size pageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                            cp.Measure(pageSize);
                            cp.Arrange(new Rect(new Point(0, 0), pageSize));
                            cp.UpdateLayout();
                            collator.Write(cp, pd.PrintTicket);
                        }));
                    }

                    Dispatcher.Invoke(new Action(delegate()
                    {
                        cp = new Membership.CardPage();
                        for (grow = 0; grow < 5; grow++)
                        {
                            for (gcol = 0; gcol < 2; gcol++)
                            {
                                Membership.Card c = new Membership.Card();
                                Size cardSize = new Size(324, 204);
                                c.Measure(cardSize);
                                c.Arrange(new Rect(new Point(0, 0), cardSize));
                                c.UpdateLayout();
                                cp.Cards.Children.Add(c);
                                Grid.SetColumn(c, gcol);
                                Grid.SetRow(c, grow);
                            }
                        }

                        Size pageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                        cp.Measure(pageSize);
                        cp.Arrange(new Rect(new Point(0, 0), pageSize));
                        cp.UpdateLayout();
                        collator.Write(cp, pd.PrintTicket);
                        collator.Write(cp, pd.PrintTicket);
                        collator.EndBatchWrite();
                    }));

                    w.CloseWindow();
                });
            }
        }
    }
}
