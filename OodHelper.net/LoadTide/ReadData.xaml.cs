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
using Microsoft.Win32;

namespace OodHelper.LoadTide
{
    /// <summary>
    /// Interaction logic for ReadData.xaml
    /// </summary>
    public partial class ReadData : Window
    {
        ReadFormat11 TideInfo { get; set; }

        public ReadData()
        {
            InitializeComponent();
            TideInfo = new ReadFormat11();
            DataContext = TideInfo;
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Format11 files; (*.tph)|*.tph|All files (*.*)|*.*";
            if (fd.ShowDialog() == true)
            {
                TideInfo.Load(fd.FileName);
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            Website.UploadTide upload = new Website.UploadTide(TideInfo.Data);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
