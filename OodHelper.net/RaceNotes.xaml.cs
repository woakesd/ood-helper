using System.Windows;
using OodHelper.ViewModels;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for RaceNotes.xaml. A thin dialog over <see cref="RaceNotesViewModel"/>;
    /// the data access lives in the view-model / repository (no <c>Db</c> in code-behind).
    /// </summary>
    public partial class RaceNotes : Window
    {
        public RaceNotes(RaceNotesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += result => DialogResult = result;
        }
    }
}
