using System;
using System.Collections;
using System.ComponentModel;
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
    public partial class DoSunSetRise : Window, INotifyPropertyChanged
    {
        private DataTable sr;
        public DataView SunDataView { get { return sr.DefaultView; } }

        public DoSunSetRise()
        {
            CalculateSunData();
            DataContext = this;
        }

        private void CalculateSunData()
        {
            sr = new DataTable();

            sr.Columns.Add("date", typeof(DateTime));
            sr.Columns.Add("sunrise", typeof(DateTime));
            sr.Columns.Add("sunset", typeof(DateTime));
            InitializeComponent();
            Sun calc = new Sun(55.996700991558, -3.409237861633301);
            string yr = Year.SelectedItem as string;
            DateTime d = new DateTime(Int32.Parse((Year.SelectedItem as ComboBoxItem).Tag as string), 1, 1);
            DateTime e = d.AddYears(1);
            DateTime? r,s;
            while (d < e)
            {
                calc.Calc(d, out r, out s);
                DataRow dr = sr.NewRow();
                dr["date"] = d;
                dr["sunrise"] = r;
                dr["sunset"] = s;
                sr.Rows.Add(dr);
                d = d.AddDays(1);
            }
        }

        private void UploadSun_Click(object sender, RoutedEventArgs e)
        {
            Website.UploadSun us = new Website.UploadSun(sr);
        }

        private void Year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateSunData();
            OnPropertyChanged("SunDataView");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
