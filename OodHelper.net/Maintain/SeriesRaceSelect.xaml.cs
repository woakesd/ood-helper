using System.Windows;
using System.Windows.Input;
using OodHelper.ViewModels;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for SeriesRaceSelect.xaml
    /// </summary>
    public partial class SeriesRaceSelect : Window
    {
        private readonly SeriesRaceSelectViewModel _viewModel;

        public SeriesRaceSelect(SeriesRaceSelectViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            viewModel.CloseRequested += result => DialogResult = result;
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
    }
}
