using OodHelper.ViewModels;

namespace OodHelper
{
    /// <summary>
    ///     Interaction logic for PNImport.xaml
    /// </summary>
    public partial class PnImport
    {
        public PnImport(PnImportViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
