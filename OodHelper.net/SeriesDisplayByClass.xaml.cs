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
using System.Windows.Markup;
using System.Windows.Xps;
using System.Printing;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for SeriesDisplayByClass.xaml
    /// </summary>
    public partial class SeriesDisplayByClass : UserControl
    {
        List<SeriesDisplay> sds = new List<SeriesDisplay>();

        public SeriesDisplayByClass(RaceSeriesResult rs)
        {
            InitializeComponent();
            foreach (string className in rs.SeriesResults.Keys)
            {
                SeriesDisplay sd = new SeriesDisplay(rs.SeriesResults[className]);
                sds.Add(sd);
                TabItem t = new TabItem();
                t.Header = className;
                t.Content = sd;
                SeriesTabControl.Items.Add(t);
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                Working w = new Working();
                w.Show();
                XpsDocumentWriter write = PrintQueue.CreateXpsDocumentWriter(pd.PrintQueue);
                VisualsToXpsDocument collator = write.CreateVisualsCollator() as VisualsToXpsDocument;
                Size ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                collator.BeginBatchWrite();
                System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        w.SetRange(0, sds.Count);
                        for (int i = 0; i < sds.Count; i++)
                        {
                            SeriesDisplay sd = sds[i];
                            string msg = null;
                            Dispatcher.Invoke(new Action(delegate()
                            {
                                msg = string.Format("Printing {0}", sd.seriesName.Content);
                            }));
                            w.SetProgress(msg, i + 1);
                            System.Threading.Thread.Sleep(50);
                            Dispatcher.Invoke(new Action(delegate()
                            {
                                SeriesDisplayPage p = new SeriesDisplayPage(sd);

                                p.Measure(ps);
                                p.Arrange(new Rect(new Point(0, 0), ps));
                                p.UpdateLayout();

                                collator.Write(p);
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

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DockPanel d = Parent as DockPanel;
            if (d != null)
                d.Children.Remove(this);
            else
            {
                TabItem t = Parent as TabItem;
                TabControl tc = t.Parent as TabControl;
                if (tc != null)
                    tc.Items.Remove(t);
                t.Content = null;
            }
        }
    }
}
