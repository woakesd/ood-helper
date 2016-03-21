using System.Collections.Generic;
using System.Windows;

namespace OodHelper.Rules
{
    /// <summary>
    ///     Interaction logic for RuleSelector.xaml
    /// </summary>
    public partial class RuleSelector
    {
        public RuleSelector(IEnumerable<BoatSelectRule> rules)
        {
            InitializeComponent();
            RuleChoice.ItemsSource = rules;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (RuleChoice.SelectedItem != null)
            {
                DialogResult = true;
                Close();
            }
            else
                MessageBox.Show("Click on a class and then click OK", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}