using System.Windows;
using OodHelper.ViewModels;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Handicap.xaml
    /// </summary>
    public partial class Handicap : Window
    {
        public Handicap(HandicapEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += result => DialogResult = result;
        }
    }
}
