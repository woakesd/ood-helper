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
            if (pd.ShowDialog() == true)
            {
                foreach (SeriesDisplay sd in sds)
                {
                    Size ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                    SeriesDisplayPage p = new SeriesDisplayPage(sd);

                    p.Measure(ps);
                    p.Arrange(new Rect(new Point(0, 0), ps));
                    p.UpdateLayout();

                    pd.PrintVisual(p, "Series result");
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ((DockPanel)Parent).Children.Remove(this);
        }
    }
}
