using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for ShowException.xaml
    /// </summary>
    public partial class ShowException : Window
    {
        public ShowException(Exception Except)
        {
            DataContext = Except;
            if (App.Current.MainWindow != this && App.Current.MainWindow.IsArrangeValid)
                Owner = App.Current.MainWindow;
            InitializeComponent();
        }
    }
}
