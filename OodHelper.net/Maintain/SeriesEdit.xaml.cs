using System.Windows;
using OodHelper.ViewModels;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for SeriesEdit.xaml
    /// </summary>
    public partial class SeriesEdit : Window
    {
        public SeriesEdit(SeriesEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += result => DialogResult = result;
        }
    }
}
