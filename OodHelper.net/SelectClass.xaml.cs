using System.Windows;
using System.Windows.Input;
using OodHelper.ViewModels;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for SelectClass.xaml
    /// </summary>
    public partial class SelectClass : Window
    {
        private readonly SelectClassViewModel _viewModel;

        public SelectClass(SelectClassViewModel viewModel)
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
                Classname.Focus();
        }
    }
}
