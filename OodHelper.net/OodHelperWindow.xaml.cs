using System.Windows;
using OodHelper.Services;
using OodHelper.ViewModels;
using OodHelper.Website;

namespace OodHelper
{
    /// <summary>
    ///     Interaction logic for Window1.xaml
    /// </summary>
    public partial class OodHelperWindow
    {
        public OodHelperWindow(OodHelperWindowViewModel viewModel, ITabHost tabHost)
        {
            InitializeComponent();
            tabHost.Attach(dock);
            DataContext = viewModel;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var c = new CheckForUpdates();
            if (c.LocalDate < c.RemoteDate)
            {
                if (MessageBox.Show("Website results are more up to date\nWould you like to download from Website",
                    "Confirm Download",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK)
                {
                    new DownloadResults();
                }
            }
        }
    }
}
