using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace OodHelper.Sun
{
    /// <summary>
    /// Interaction logic for ReadData.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class DoSunSetRise : Window
    {
        private class Data: NotifyPropertyChanged
        {
            public DataTable sr;
            public DataView SunDataView { get { return sr.DefaultView; } }
        }

        private Data dc = new Data();

        public DoSunSetRise()
        {
            CalculateSunData();
            DataContext = dc;
        }

        private void CalculateSunData()
        {
            dc.sr = new DataTable();

            dc.sr.Columns.Add("date", typeof(DateTime));
            dc.sr.Columns.Add("sunrise", typeof(DateTime));
            dc.sr.Columns.Add("sunset", typeof(DateTime));
            InitializeComponent();
            Sun calc = new Sun(55.996700991558, -3.409237861633301);
            string yr = Year.SelectedItem as string;
            DateTime d = new DateTime(Int32.Parse((Year.SelectedItem as ComboBoxItem).Tag as string), 1, 1);
            DateTime e = d.AddYears(1);
            DateTime? r,s;
            while (d < e)
            {
                calc.Calc(d, out r, out s);
                DataRow dr = dc.sr.NewRow();
                dr["date"] = d;
                dr["sunrise"] = r;
                dr["sunset"] = s;
                dc.sr.Rows.Add(dr);
                d = d.AddDays(1);
            }
        }

        private void UploadSun_Click(object sender, RoutedEventArgs e)
        {
            Website.UploadSun us = new Website.UploadSun(dc.sr);
        }

        private void Year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateSunData();
            dc.OnPropertyChanged("SunDataView");
        }
    }
}
