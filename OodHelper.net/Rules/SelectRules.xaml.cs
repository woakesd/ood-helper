using System.Windows;
using System.Windows.Input;
using OodHelper.ViewModels;

namespace OodHelper.Rules
{
    /// <summary>
    /// Interaction logic for SelectRules.xaml
    /// </summary>
    public partial class SelectRules : Window
    {
        private readonly SelectRulesViewModel _viewModel;

        public SelectRules(SelectRulesViewModel viewModel)
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
                Filter.Focus();
        }
    }
}
