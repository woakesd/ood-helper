using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Linq;
using System.Text;

namespace OodHelper
{
    [Svn("$Id$")]
    public class RaceEditDateEditSelector : DataTemplateSelector
    {
        public DataTemplate TimeOnly { get; set; }
        public DataTemplate DateAndTime { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return TimeOnly;
        }
    }
}
