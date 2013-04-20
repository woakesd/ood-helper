using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace OodHelper.Behaviors
{
    public class ExcelLikeBehavior : DependencyObject
    {
        public static object GetIsExcelLike(UIElement Target)
        {
            return Target.GetValue(IsExcelLikeProperty);
        }

        public static void SetIsExcelLike(UIElement Target, object Value)
        {
            Target.SetValue(IsExcelLikeProperty, Value);
        }

        public static readonly DependencyProperty IsExcelLikeProperty = DependencyProperty.RegisterAttached("IsExcelLike",
            typeof(bool), typeof(ExcelLikeBehavior), new UIPropertyMetadata(false, OnIsExcelLike));

        public static void OnIsExcelLike(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            if (e == null) return;
            if ((bool)e.NewValue)
            {
                DataGridCell _dgc = depObj as DataGridCell;
                if (_dgc != null)
                {
                    _dgc.PreviewMouseLeftButtonDown += _dgc_PreviewMouseLeftButtonDown;
                    _dgc.PreviewKeyDown += _dgc_PreviewKeyDown;
                    _dgc.KeyUp += _dgc_KeyUp;
                }
            }
            else
            {
                DataGridCell _dgc = depObj as DataGridCell;
                if (_dgc != null)
                {
                    _dgc.PreviewMouseLeftButtonDown -= _dgc_PreviewMouseLeftButtonDown;
                    _dgc.PreviewKeyDown -= _dgc_PreviewKeyDown;
                    _dgc.KeyUp -= _dgc_KeyUp;
                }
            }
        }

        public static void _dgc_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                DataGridCell cell = sender as DataGridCell;
                if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
                {
                    cell.Focus();
                    DataGrid _dg = FindVisualParent<DataGrid>(cell);
                    if (_dg != null)
                        _dg.BeginEdit();
                }
            }
        }

        private static void _dgc_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                DataGridCell cell = sender as DataGridCell;
                if (cell != null)
                {
                    TextBox c = cell.Content as TextBox;
                    if (c == null || ((e.Key != Key.Right || c.SelectionStart != 0 || c.Text.Length == 0)
                        && (e.Key != Key.Left && e.Key != Key.Right || c.SelectionStart == 0 || c.SelectionStart + c.SelectionLength >= c.Text.Length)
                        && (e.Key != Key.Left || c.SelectionStart + c.SelectionLength < c.Text.Length || c.Text.Length == 0)))
                    {
                        DataGrid _dg = FindVisualParent<DataGrid>(cell);
                        if (_dg != null)
                            _dg.CommitEdit();
                        cell.IsEditing = false;
                    }
                }
                e.Handled = false;
            }
        }

        private static void _dgc_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                    cell.IsSelected = true;
                    DataGrid _dg = FindVisualParent<DataGrid>(cell);
                    if (_dg != null)
                        _dg.BeginEdit();
                }
            }
        }

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
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
    }
}