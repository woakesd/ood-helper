using System.Windows;
using System.Windows.Input;
using OodHelper.ViewModels;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Races.xaml
    /// </summary>
    public partial class Races : Window
    {
        private readonly RacesViewModel _viewModel;

        public Races(RacesViewModel viewModel)
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

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                Eventname.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
