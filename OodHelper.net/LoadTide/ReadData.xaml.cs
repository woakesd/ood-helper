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

namespace OodHelper.LoadTide
{
    /// <summary>
    /// Interaction logic for ReadData.xaml
    /// </summary>
    public partial class ReadData : Window
    {
        public ReadData()
        {
            InitializeComponent();
            ReadAutoTideData r = new ReadAutoTideData(@"C:\Documents and Settings\david\My Documents\Peyc Data\at-rosyth-2011.txt");
            TideTable.ItemsSource = r.Data.DefaultView;
        }
    }
}
