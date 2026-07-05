using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for Working.xaml
    /// </summary>
    public partial class Working : Window
    {
        public Working(Window Parent)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Left = Owner.Left + Owner.ActualWidth / 2 - Width / 2;
            Top = Owner.Top + Owner.ActualHeight / 2 - Height / 2;
            Progress.IsIndeterminate = true;
        }

        private CancellationTokenSource? _cts;

        //
        // Async/await variant: the caller drives progress via SetProgress and cancellation flows
        // through the supplied CancellationTokenSource (used by the EF download/upload and scoring).
        //
        public Working(Window Parent, CancellationTokenSource cts) : this(Parent)
        {
            _cts = cts;
            CancelButton.Visibility = Visibility.Visible;
            Progress.IsIndeterminate = false;
            Progress.Minimum = 0;
            Progress.Maximum = 100;
        }

        public void SetProgress(string message, int value)
        {
            Dispatcher.Invoke(delegate() { Progress.Value = value; Message.Text = message; });
        }

        public void SetRange(int min, int max)
        {
            Dispatcher.Invoke(delegate() { Progress.Minimum = min; Progress.Maximum = max; if (min < max) Progress.IsIndeterminate = false; });
        }

        public void CloseWindow()
        {
            Dispatcher.Invoke(delegate() { Close(); });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }
    }
}
