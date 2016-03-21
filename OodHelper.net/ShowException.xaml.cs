using System;
using System.Windows;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for ShowException.xaml
    /// </summary>
    public partial class ShowException
    {
        public static void DoShow(Exception except, string exceptionType = "Info")
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                var se = new ShowException(except);
                se.Show();
            });
        }

        public ShowException(Exception except)
        {
            DataContext = except;
            if (!Application.Current.MainWindow.Equals(this) && Application.Current.MainWindow.IsArrangeValid)
                Owner = Application.Current.MainWindow;
            InitializeComponent();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
