using System.Windows;
using OodHelper.ViewModels;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configure : Window
    {
        public Configure(ConfigurationViewModel viewModel)
        {
            Owner = App.Current.MainWindow;
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += result => DialogResult = result;
        }
    }
}
