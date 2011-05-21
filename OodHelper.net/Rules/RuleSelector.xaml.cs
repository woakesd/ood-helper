using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OodHelper;

namespace OodHelper.Rules
{
    /// <summary>
    /// Interaction logic for RuleSelector.xaml
    /// </summary>
    public partial class RuleSelector : Window
    {
        public RuleSelector(List<BoatSelectRule> rules)
        {
            InitializeComponent();
            RuleChoice.ItemsSource = rules;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (RuleChoice.SelectedItem != null)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
                MessageBox.Show("Click on a class and then click OK", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
