using System.Windows;
using System.Windows.Input;
using OodHelper.ViewModels;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Boats.xaml
    /// </summary>
    public partial class Boats : Window
    {
        public Boats(BoatsViewModel viewModel)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Width = SystemParameters.MaximizedPrimaryScreenWidth * 0.8;
            Height = SystemParameters.MaximizedPrimaryScreenHeight * 0.8;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DataContext = viewModel;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                Boatname.Focus();
        }
    }
}
