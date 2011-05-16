using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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

namespace OodHelper.Rules
{
    /// <summary>
    /// Interaction logic for SetupRules.xaml
    /// </summary>
    public partial class SelectRuleEdit : Window
    {
        private BoatSelectRule Root;

        public SelectRuleEdit(Guid? id)
        {
            InitializeComponent();
            Root = new BoatSelectRule();
            if (id.HasValue)
            {
                Root = new BoatSelectRule(id);
                RuleName.Text = Root.Name;
            }
            else
            {
                Root.Application = Apply.Any;
                BoatSelectRule c = new BoatSelectRule();
                c.Condition = ConditionType.Equals;
                Root.Add(c);
            }

            List<SelectRuleModelView> rm = new List<SelectRuleModelView>();
            rm.Add(new SelectRuleModelView(Root));
            Rules.ItemsSource = rm;

            Array values = System.Enum.GetValues(typeof(ConditionType));
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox me = sender as ComboBox;
            if (me != null)
            {
                Size inf = new Size(double.PositiveInfinity, double.PositiveInfinity);
                double cbWidth = 0.0;
                double selectedItemWidth = 0.0;
                foreach (object s in me.Items)
                {
                    ComboBoxItem i = new ComboBoxItem();
                    i.Content = s;
                    i.Measure(inf);
                    if (i.DesiredSize.Width > cbWidth)
                        cbWidth = i.DesiredSize.Width;
                    if (me.SelectedItem != null && s.ToString() == me.SelectedItem.ToString())
                        selectedItemWidth = i.DesiredSize.Width;
                }
                me.Measure(inf);
                me.Width = me.DesiredSize.Width + cbWidth - selectedItemWidth + 4;
            }
        }

        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        
        private void AddSibling_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem t = FindVisualParent<TreeViewItem>(sender as UIElement);
            if (t != null)
            {
                SelectRuleModelView s = t.DataContext as SelectRuleModelView;
                BoatSelectRule parent = s.Rule.Parent; 
                BoatSelectRule b = new BoatSelectRule();
                parent.Add(b);
                s.AddSibling(new SelectRuleModelView(b, s.Parent));
            }
        }

        private void RemoveMe_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem t = FindVisualParent<TreeViewItem>(sender as UIElement);
            if (t != null)
            {
                SelectRuleModelView b = t.DataContext as SelectRuleModelView;
                if (b != null)
                    b.RemoveFromParent();
            }
        }

        private void AddParentSibling_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem t = FindVisualParent<TreeViewItem>(sender as UIElement);
            if (t != null)
            {
                SelectRuleModelView s = t.DataContext as SelectRuleModelView;
                BoatSelectRule parent = s.Rule.Parent;
                BoatSelectRule b = new BoatSelectRule();
                parent.Add(b);
                BoatSelectRule c = new BoatSelectRule();
                b.Add(c);
                s.AddSibling(new SelectRuleModelView(b, s.Parent));
            }
        }

        private void RecurseControls(TreeViewItem tvi, SelectRuleModelView mv)
        {
            ContentPresenter cp = FindVisualChild<ContentPresenter>(tvi);
            TextBox Bound1 = Rules.ItemTemplate.FindName("Bound1", cp) as TextBox;

            BindingExpression be = Bound1.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();

            foreach (SelectRuleModelView ch in mv.Children)
            {
                TreeViewItem childtvi = (TreeViewItem)tvi.ItemContainerGenerator.ContainerFromItem(ch);
                RecurseControls(childtvi, ch);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            foreach (SelectRuleModelView mv in Rules.Items)
            {
                TreeViewItem tvi = (TreeViewItem)Rules.ItemContainerGenerator.ContainerFromItem(mv);
                RecurseControls(tvi, mv);
            }

            Root.Name = RuleName.Text;
            Root.Save();
            DialogResult = true;
            Close();
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
