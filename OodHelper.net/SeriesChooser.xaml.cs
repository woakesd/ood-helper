using System;
using System.Collections;
using System.Collections.Generic;
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

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for SeriesChooser.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class SeriesChooser : Window
    {
        private DataTable cal;
        Working w;

        public SeriesChooser()
        {
            InitializeComponent();
        }

        private delegate void DSetGridSource();
        private DSetGridSource dSetGridSource;

        void SeriesChooser_Loaded(object sender, RoutedEventArgs e)
        {
            w = new Working(this);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Db d = new Db("SELECT * " +
                        "FROM series " +
                        "ORDER BY sname");
                    cal = d.GetData(null);
                    Dispatcher.Invoke(dSetGridSource = SetGridSource, null);
                });
        }

        private void SetGridSource()
        {
            CalGrid.ItemsSource = cal.DefaultView;

            w.Close();
        }

        void cal_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            setChosenSeries();
        }

        private void buttonResults_Click(object sender, RoutedEventArgs e)
        {
            setChosenSeries();
        }

        public int Sid
        {
            get
            {
                return sid;
            }
        }

        private int sid;

        private void setChosenSeries()
        {
            int rowIndex = CalGrid.SelectedIndex;
            sid = (int)cal.Rows[rowIndex]["sid"];
            this.DialogResult = true;
        }
    }
}
