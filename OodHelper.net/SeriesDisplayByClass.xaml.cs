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

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for SeriesDisplayByClass.xaml
    /// </summary>
    [Svn("$Id$")]
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
            PrintPreview pv = new PrintPreview();

            FixedDocument fd = new FixedDocument();
            fd.DocumentPaginator.PageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
            foreach (SeriesDisplay sd in sds)
            {
                FixedPage p1 = new FixedPage();
                p1.Width = fd.DocumentPaginator.PageSize.Width;
                p1.Height = fd.DocumentPaginator.PageSize.Height;

                Frame f = new Frame();
                f.Width = p1.Width;
                f.HorizontalAlignment = HorizontalAlignment.Stretch;
                Page p = null;

                        p = (Page)new SeriesDisplayPage(sd);
                        p.Width = p1.Width;

                f.Navigate(p);

                p1.Children.Add(f);

                PageContent pc = new PageContent();
                IAddChild ac = pc as IAddChild;
                ac.AddChild(p1);

                fd.Pages.Add(pc);
            }
            pv.Viewer.Document = fd;
            pv.Viewer.FitToWidth();
            pv.ShowDialog();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ((DockPanel)Parent).Children.Remove(this);
        }
    }
}
