using System.Windows;
using System.Windows.Controls;

namespace OodHelper.Behaviors
{
    /// <summary>
    /// Sizes a ComboBox to fit its widest item (otherwise the rule-editor combos size only to the
    /// currently selected item). Replaces the old ComboBox_Loaded code-behind handler.
    /// </summary>
    public static class ComboBoxAutoWidth
    {
        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled", typeof(bool), typeof(ComboBoxAutoWidth), new PropertyMetadata(false, OnEnabledChanged));

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox cb && (bool)e.NewValue)
                cb.Loaded += OnLoaded;
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var me = sender as ComboBox;
            if (me == null) return;

            var inf = new Size(double.PositiveInfinity, double.PositiveInfinity);
            double cbWidth = 0.0;
            double selectedItemWidth = 0.0;
            foreach (object s in me.Items)
            {
                var i = new ComboBoxItem { Content = s };
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
}
