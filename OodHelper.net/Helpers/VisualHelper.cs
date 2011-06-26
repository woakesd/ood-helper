using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace OodHelper.Helpers
{
    class VisualHelper
    {
        //
        // If a user changes the content of a text box and then hits return
        // then an Click trigger can be fired without the text box firing it's lost focus
        // trigger, so we need to update the source for the text boxes.
        //
        static public void UpdateTextBoxSources(ContentControl p)
        {
            List<TextBox> tboxes = VisualHelper.FindVisualChild<TextBox>(p);
            foreach (TextBox tb in tboxes)
            {
                BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
                if (be != null) be.UpdateSource();
            }
        }
        
        static public List<childItem> FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            List<childItem> ret = new List<childItem>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    ret.Add((childItem)child);
                else
                {
                    List<childItem> childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild.Count != 0)
                        ret.AddRange(childOfChild);
                }
            }
            return ret;
        }
    }
}
