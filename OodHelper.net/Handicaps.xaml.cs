using System.Windows;
using System.Windows.Input;
using OodHelper.ViewModels;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Handicaps.xaml
    /// </summary>
    public partial class Handicaps : Window
    {
        private readonly HandicapsViewModel _viewModel;

        public Handicaps(HandicapsViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Load();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                ClassName.Focus();
        }
    }
}
