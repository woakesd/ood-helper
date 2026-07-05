using System.Windows;
using OodHelper.ViewModels;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for RaceEdit.xaml
    /// </summary>
    public partial class RaceEdit : Window
    {
        public RaceEdit(RaceEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += result => DialogResult = result;
        }
    }
}
