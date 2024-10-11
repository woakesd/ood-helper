using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace OodHelper
{
    internal class Common
    {
        public static List<T> FindVisualChild<T>(UIElement element) where T : UIElement
        {
            var r = new List<T>();
            var child = element;
            if (child == null) return r;
            var correctlyTyped = child as T;
            if (correctlyTyped != null)
            {
                r.Add(correctlyTyped);
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++)
            {
                var subchild = VisualTreeHelper.GetChild(child, i) as UIElement;
                var subList = FindVisualChild<T>(subchild!);
                r.InsertRange(r.Count, subList);
            }
            return r;
        }

        public static string HMS(double t)
        {
            var tmp = new TimeSpan((long)(t * 10 * 1000 * 1000));
            return tmp.ToString(@"hh\:mm\:ss");
        }
    }
}