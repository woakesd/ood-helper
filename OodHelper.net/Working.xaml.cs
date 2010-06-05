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
    [Svn("$Id$")]
    public partial class Working : Window
    {
        private delegate void DSetProgress(string message, int value);
        private DSetProgress dSetProgress;

        private delegate void DCloseWindow();
        private DCloseWindow dCloseWindow;

        public Working(Window c)
        {
            InitializeComponent();
            Show();
            Left = c.Left + c.Width / 2 - Width / 2;
            Top = c.Top + c.Height / 2 - Height / 2;
            Progress.IsIndeterminate = true;
            dSetProgress = _setProgress;
            dCloseWindow = _closeWindow;
        }

        public Working(Window c, string initialMessage, bool isIndeterminate, int min, int max)
        {
            InitializeComponent();
            Show();
            Left = c.Left + c.Width / 2 - Width / 2;
            Top = c.Top + c.Height / 2 - Height / 2;
            Message.Text = initialMessage;
            Progress.IsIndeterminate = isIndeterminate;
            Progress.Minimum = min;
            Progress.Maximum = max;
            dSetProgress = _setProgress;
            dCloseWindow = _closeWindow;
        }

        private void _setProgress(string message, int value)
        {
            Progress.Value = value;
            Message.Text = message;
        }

        private void _closeWindow()
        {
            Close();
        }

        public void SetProgress(string message, int value)
        {
            Dispatcher.Invoke(dSetProgress, new object[] { message, value });
        }

        public void CloseWindow()
        {
            Dispatcher.Invoke(dCloseWindow);
        }
    }
}
