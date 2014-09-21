using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace OodHelper.Rules
{
    /// <summary>
    ///     Interaction logic for SetupRules.xaml
    /// </summary>
    public partial class SelectRuleEdit
    {
        private readonly BoatSelectRule _root;

        public SelectRuleEdit(Guid? id)
        {
            InitializeComponent();
            _root = new BoatSelectRule();
            if (id.HasValue)
            {
                _root = new BoatSelectRule(id);
                RuleName.Text = _root.Name;
            }
            else
            {
                _root.Application = Apply.Any;
                var c = new BoatSelectRule {Condition = ConditionType.Equals};
                _root.Add(c);
            }

            var rm = new List<SelectRuleModelView> {new SelectRuleModelView(_root)};
            Rules.ItemsSource = rm;
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var me = sender as ComboBox;
            if (me != null)
            {
                var inf = new Size(double.PositiveInfinity, double.PositiveInfinity);
                double cbWidth = 0.0;
                double selectedItemWidth = 0.0;
                foreach (object s in me.Items)
                {
                    var i = new ComboBoxItem {Content = s};
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

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
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
            var t = FindVisualParent<TreeViewItem>(sender as UIElement);
            if (t != null)
            {
                var s = t.DataContext as SelectRuleModelView;
                if (s == null) return;
                var parent = s.Rule.Parent;
                var b = new BoatSelectRule();
                parent.Add(b);
                s.AddSibling(new SelectRuleModelView(b, s.Parent));
            }
        }

        private void RemoveMe_Click(object sender, RoutedEventArgs e)
        {
            var t = FindVisualParent<TreeViewItem>(sender as UIElement);
            if (t != null)
            {
                var b = t.DataContext as SelectRuleModelView;
                if (b != null)
                    b.RemoveFromParent();
            }
        }

        private void AddParentSibling_Click(object sender, RoutedEventArgs e)
        {
            var t = FindVisualParent<TreeViewItem>(sender as UIElement);
            if (t != null)
            {
                var s = t.DataContext as SelectRuleModelView;
                if (s == null) return;
                var parent = s.Rule.Parent;
                var b = new BoatSelectRule();
                parent.Add(b);
                var c = new BoatSelectRule();
                b.Add(c);
                s.AddSibling(new SelectRuleModelView(b, s.Parent));
            }
        }

        private void TextBoxUpdateSource(string name, ContentPresenter cp)
        {
            var tb = Rules.ItemTemplate.FindName(name, cp) as TextBox;
            if (tb != null)
            {
                BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
                if (be != null) be.UpdateSource();
            }
        }

        private void RecurseControls(TreeViewItem tvi, SelectRuleModelView mv)
        {
            var cp = FindVisualChild<ContentPresenter>(tvi);
            TextBoxUpdateSource("Bound1", cp);
            TextBoxUpdateSource("Bound2", cp);
            TextBoxUpdateSource("StringValue", cp);

            foreach (SelectRuleModelView ch in mv.Children)
            {
                var childtvi = (TreeViewItem) tvi.ItemContainerGenerator.ContainerFromItem(ch);
                RecurseControls(childtvi, ch);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            foreach (SelectRuleModelView mv in Rules.Items)
            {
                var tvi = (TreeViewItem) Rules.ItemContainerGenerator.ContainerFromItem(mv);
                RecurseControls(tvi, mv);
            }

            _root.Name = RuleName.Text;
            _root.Save();
            DialogResult = true;
            Close();
        }

        private TChildItem FindVisualChild<TChildItem>(DependencyObject obj)
            where TChildItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is TChildItem)
                    return (TChildItem) child;
                var childOfChild = FindVisualChild<TChildItem>(child);
                if (childOfChild != null)
                    return childOfChild;
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