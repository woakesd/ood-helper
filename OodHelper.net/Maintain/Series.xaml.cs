using System.Windows;
using System.Windows.Input;
using OodHelper.ViewModels;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Series.xaml
    /// </summary>
    public partial class Series : Window
    {
        private readonly SeriesViewModel _viewModel;

        public Series(SeriesViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.Load();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                FilterText.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
