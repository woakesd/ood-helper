using System.Windows;
using OodHelper.ViewModels;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for SeriesChooser.xaml
    /// </summary>
    public partial class SeriesChooser : Window
    {
        private readonly SeriesChooserViewModel _viewModel;

        public SeriesChooser(SeriesChooserViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            viewModel.CloseRequested += result => DialogResult = result;
        }

        private void SeriesChooser_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Load();
        }
    }
}
