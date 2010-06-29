using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Markup;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for RaceEdit.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class SeriesDisplay : UserControl
    {
        private DataTable rd;
        private Hashtable caldata;

        public SeriesDisplay(SeriesResult r)
        {
            InitializeComponent();

            _result = r;

            LoadGrid();
        }

        SeriesResult _result;

        private int CompareByDate(SeriesEvent x, SeriesEvent y)
        {
            if (x.Date > y.Date)
                return 1;
            else if (x.Date < y.Date)
                return -1;
            return 0;
        }

        public void LoadGrid()
        {
            rd = new DataTable();
            rd.Columns.Add("bid", typeof(int));
            rd.Columns.Add("boatname", typeof(string));
            rd.Columns.Add("boatclass", typeof(string));
            rd.Columns.Add("sailno", typeof(string));
            rd.Columns.Add("entered", typeof(double));
            rd.Columns.Add("score", typeof(double));

            List<SeriesEvent> events = new List<SeriesEvent>(_result._SeriesData.Values);
            events.Sort(CompareByDate);
            int i = 1;
            foreach (SeriesEvent se in events)
            {
                rd.Columns.Add("r" + i.ToString(), typeof(SeriesEntry));

                int k = rd.Columns["R" + i].Ordinal;

                DataGridTemplateColumn x = new DataGridTemplateColumn();
                UTF8Encoding ue = new UTF8Encoding();

                x.Header = "R" + i;
                string dataTemplate = @"<?xml version=""1.0"" encoding=""utf-8""?>
                        <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                            <StackPanel Orientation=""Horizontal"" Name=""panel"">
                                <TextBlock Text=""{Binding r" + i + @".Points, StringFormat=#.#}"" />
                                <TextBlock Text=""{Binding r" + i + @".CodeDisplay}"" FontSize=""9""/>
                            </StackPanel>
                            <DataTemplate.Triggers>
                                <DataTrigger Binding=""{Binding r" + i + @".Discard}"" Value=""True"">
                                    <DataTrigger.Setters>
                                        <Setter Property=""Background"" Value=""LightGray"" TargetName=""panel""/>
                                    </DataTrigger.Setters>
                                </DataTrigger>
                            </DataTemplate.Triggers>
                        </DataTemplate>";
                x.CellTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Load(new System.IO.MemoryStream(ue.GetBytes(dataTemplate)));
                Races.Columns.Add(x);

                i++;
            }

            Db c = new Db("SELECT boatname, boatclass, sailno " +
                "FROM boats " +
                "WHERE bid = @bid");
            Hashtable p = new Hashtable();
            foreach (BoatSeriesResult bsr in _result._Results)
            {
                DataRow r = rd.NewRow();
                r["bid"] = bsr.bid;
                r["entered"] = bsr.count;
                r["score"] = bsr.nett;

                p["bid"] = bsr.bid;
                Hashtable bd = c.GetHashtable(p);
                if (bd.Count > 0)
                {
                    r["boatname"] = bd["boatname"];
                    r["boatclass"] = bd["boatclass"];
                    r["sailno"] = bd["sailno"];
                }

                i = 1;
                foreach (SeriesEntry sent in bsr.dateSortedPoints)
                {
                    r["r" + i] = sent;
                    i++;
                }
                rd.Rows.Add(r);
                DataGridRow gr = new DataGridRow();
                gr.Item = r;
            }
            rd.AcceptChanges();

            Races.ItemsSource = rd.DefaultView;
        }

        private void buttonCalculate_Click(object sender, RoutedEventArgs e)
        {
        }

    }

    class SeriesEntryDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != DBNull.Value)
            {
                SeriesEntry sent = (SeriesEntry)value;
                if (sent.code == null || sent.code == string.Empty)
                    return sent.Points.ToString("#.#");
                else
                    return sent.Points.ToString("#.#") + "(" + sent.code + ")";
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
