using System;
using System.Data;
using System.Printing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Xps;
using OodHelper.Membership;
using OodHelper.Services;
using OodHelper.ViewModels;
using OodHelper.Website;

namespace OodHelper
{
    /// <summary>
    ///     Interaction logic for Window1.xaml
    /// </summary>
    public partial class OodHelperWindow
    {
        public OodHelperWindow(OodHelperWindowViewModel viewModel, ITabHost tabHost)
        {
            InitializeComponent();
            tabHost.Attach(dock);
            DataContext = viewModel;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
                XpsDocumentWriter write = PrintQueue.CreateXpsDocumentWriter(pd.PrintQueue);

                var collator = write.CreateVisualsCollator() as VisualsToXpsDocument;
                if (collator == null) return;

                using (Task.Factory.StartNew(() =>
                {
                    Dispatcher.Invoke(collator.BeginBatchWrite);

                    w.SetProgress("Printing ", 0);
                    Thread.Sleep(50);

                    CardPage cp = null;
                    Dispatcher.Invoke(delegate { cp = new CardPage(); });

                    int grow;
                    int gcol;
                    int index;

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
                            var c = new Card(string.Format("{0} {1}", new[] {dr["firstname"], dr["surname"]}), (int) dr["id"], (int) dr["main_id"], dr["member"] as string);
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
                }))
                {
                }
            }
        }
    }
}
