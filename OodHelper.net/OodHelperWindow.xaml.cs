using System.Windows;
using OodHelper.Services;
using OodHelper.ViewModels;

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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is OodHelperWindowViewModel vm)
                await vm.CheckForUpdatesAsync();
        }
    }
}
