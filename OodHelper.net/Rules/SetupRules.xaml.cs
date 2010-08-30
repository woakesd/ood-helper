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

namespace OodHelper.net.Rules
{
    /// <summary>
    /// Interaction logic for SetupRules.xaml
    /// </summary>
    public partial class SetupRules : Window
    {
        public SetupRules()
        {
            InitializeComponent();
            List<BoatSelectRule> root = new List<BoatSelectRule>();
            BoatSelectRule b = new BoatSelectRule();
            b.Application = Apply.Any;
            for (int i = 0; i < 3; i++)
                b.Children.Add(new BoatSelectRule());
            root.Add(b);
            List<SelectRuleModelView> rm = new List<SelectRuleModelView>();
            rm.Add(new SelectRuleModelView(b));
            Rules.ItemsSource = rm;

            Array values = System.Enum.GetValues(typeof(ConditionType));
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
