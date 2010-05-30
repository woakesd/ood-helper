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
    /// Interaction logic for WebProgress.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class WebProgress : Window
    {
        private delegate void DSetProgress(string message, int value);
        private DSetProgress dSetProgress;

        public WebProgress()
        {
            InitializeComponent();
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 6;
            dSetProgress = SetProgress;
        }

        private void SetProgress(string message, int value)
        {
            progressBar1.Value = value;
            textBlock1.Text = message;
        }

        public void Progress(string message, int value)
        {
            Dispatcher.Invoke(dSetProgress, new object[] { message, value });
        }
    }
}
