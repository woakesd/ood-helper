using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace OodHelper.net
{
    [Svn("$Id$")]
    class Common
    {
        public static List<T> FindVisualChild<T>(UIElement element) where T : UIElement
        {
            List<T> r = new List<T>();
            UIElement child = element;
            if (child != null)
            {
                T correctlyTyped = child as T;
                if (correctlyTyped != null)
                {
                    r.Add(correctlyTyped);
                }

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++)
                {
                    UIElement subchild = VisualTreeHelper.GetChild(child, i) as UIElement;
                    List<T> subList = FindVisualChild<T>(subchild);
                    r.InsertRange(r.Count, subList);
                }
            }
            return r;
        }

        public static string HMS(double t)
        {
            if (t != 999999)
            {
                int s = (int)t % 60;
                int m = (int)t / 60;
                int h = m / 60;
                m = m % 60;
                return h.ToString().PadLeft(2, '0') + ':' +
                    m.ToString().PadLeft(2, '0') + ':' +
                    s.ToString().PadLeft(2, '0');
            }
            else
                return string.Empty;
        }
    }
}
