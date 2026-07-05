using OodHelper.ViewModels;

namespace OodHelper.Rules
{
    /// <summary>
    ///     Interaction logic for SelectRuleEdit.xaml
    /// </summary>
    public partial class SelectRuleEdit
    {
        public SelectRuleEdit(SelectRuleEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += result => DialogResult = result;
        }
    }
}
