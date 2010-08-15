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
using System.Printing;
using System.Xml;
using System.Windows.Markup;
using System.Windows.Xps;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for EntrySheetSelector.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class EntrySheetSelector : Window
    {
        public EntrySheetSelector()
        {
            InitializeComponent();
            Db c = new Db(@"SELECT 0 print_all_visible, 0 print_all, 0 [print], 1 [copies], rid, start_date, event, class
                FROM calendar
                WHERE is_race = 1
                AND raced = 0
                AND start_date BETWEEN @today
                AND DATEADD(DAY, 10, @today)
                ORDER BY start_date");
            Hashtable para = new Hashtable();
            para["today"] = DateTime.Today;
            DataTable d = c.GetData(para);
            DateTime? NextDate = null;

            if (d.Rows.Count > 0)
            {
                NextDate = d.Rows[0]["start_date"] as DateTime?;
                d.Rows[0]["print_all_visible"] = true;
                d.Rows[0]["print_all"] = true;
            }
            for (int i = 0; i < d.Rows.Count; i++)
            {
                DataRow p = null;
                if (i > 0) p = d.Rows[i-1];
                DataRow r = d.Rows[i];
                if (r["class"] as string == "Division 1")
                    r["copies"] = 2;
                DateTime? start = r["start_date"] as DateTime?;
                if (start.HasValue)
                {
                    if (start.Value.Date == NextDate.Value.Date)
                        r["print"] = true;
                    else
                        r["print"] = false;
                    if (i > 0 && ((DateTime)r["start_date"]).Date != ((DateTime)p["start_date"]).Date)
                        r["print_all_visible"] = true;
                    else if (i > 0)
                        r["print_all_visible"] = false;
                }
            }
            Races.ItemsSource = d.DefaultView;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            DataView v = Races.ItemsSource as DataView;
            if (v != null)
            {
                DataRow[] rows = v.Table.Select("print = 1");

                if (rows.Length == 0)
                {
                    Close();
                    return;
                }

                PrintDialog pd = new PrintDialog();
                pd.PrintTicket.PageOrientation = PageOrientation.Landscape;

                if (pd.ShowDialog() == true)
                {
                    Working w = new Working(App.Current.MainWindow);
                    Size ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                    XpsDocumentWriter write = PrintQueue.CreateXpsDocumentWriter(pd.PrintQueue);
                    VisualsToXpsDocument collator = write.CreateVisualsCollator() as VisualsToXpsDocument;

                    System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        w.SetRange(0, rows.Length);

                        Dispatcher.Invoke(new Action(delegate()
                        {
                            collator.BeginBatchWrite();
                        }));

                        for (int i = 0; i < rows.Length; i++)
                        {
                            DataRow r = rows[i];
                            w.SetProgress("Printing " + r["event"] + " - " + r["class"], i + 1);
                            System.Threading.Thread.Sleep(50);
                            Dispatcher.Invoke(new Action(delegate()
                            {
                                EntrySheet p = new EntrySheet((int)r["rid"]);
                                p.Width = ps.Width;
                                p.Height = ps.Height;

                                p.Measure(ps);
                                p.Arrange(new Rect(new Point(0, 0), ps));
                                p.UpdateLayout();

                                int? copies = r["copies"] as int?;
                                for (int c = 0; copies.HasValue && c < copies || c < 1; c++)
                                    collator.Write(p, pd.PrintTicket);
                            }));
                        }
                        Dispatcher.Invoke(new Action(delegate()
                        {
                            collator.EndBatchWrite();
                        }));
                        w.CloseWindow();
                    });
                }
            }

            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Include_All_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox;
            if (cb != null)
            {
                DataRowView rv = cb.DataContext as DataRowView;
                DataRow r = rv.Row;
                DataTable d = rv.DataView.Table;
                foreach (DataRow p in d.Rows)
                {
                    if (((DateTime)p["start_date"]).Date == ((DateTime)r["start_date"]).Date)
                        p["print"] = cb.IsChecked;
                }
            }
        }

        private void Include_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox;
            if (cb != null)
            {
                DataRowView rv = cb.DataContext as DataRowView;
                DataRow r = rv.Row;
                DataTable d = rv.DataView.Table;
                for (int i = 0; i < d.Rows.Count; i++)
                {
                    DataRow p = d.Rows[i];
                    if (((DateTime)p["start_date"]).Date == ((DateTime)r["start_date"]).Date
                        && (int)p["print_all_visible"] == 1
                        && cb.IsChecked == false)
                        p["print_all"] = false;
                    if ((int)p["print_all_visible"] == 1)
                    {
                        bool allprint = true;
                        for (int j = i; j < d.Rows.Count; j++)
                        {
                            DataRow q = d.Rows[j];
                            if (((DateTime)q["start_date"]).Date == ((DateTime)p["start_date"]).Date &&
                                (int)q["print"] == 0)
                            {
                                allprint = false;
                                break;
                            }
                        }
                        p["print_all"] = allprint;
                    }
                }
            }
        }
    }
}
