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

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class AboutOod
    {
        public AboutOod()
        {
            InitializeComponent();
            var _rev = System.Reflection.Assembly.GetExecutingAssembly()
                                                 .GetName().Version!
                                                 .ToString();

            aboutBlock.Text = string.Format("Revision: {0}\nOOD Helper by David Woakes", _rev);
        }
    }
}
