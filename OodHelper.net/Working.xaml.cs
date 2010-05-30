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

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for Working.xaml
    /// </summary>
    public partial class Working : Window
    {
        [Svn("$Id$")]
        public Working(Window c)
        {
            InitializeComponent();
            Show();
            Left = c.Left + c.Width / 2 - Width / 2;
            Top = c.Top + c.Height / 2 - Height / 2;
        }
    }
}
