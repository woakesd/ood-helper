using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public Working(Window c)
        {
            InitializeComponent();
            Left = c.Left + c.ActualWidth / 2 - Width / 2;
            Top = c.Top + c.ActualHeight / 2 - Height / 2;
            Progress.IsIndeterminate = true;
        }

        public Working() : this (Application.Current.MainWindow)
        {
        }

        private BackgroundWorker worker { get; set; }

        public Working(BackgroundWorker w) : this()
        {
            worker = w;
            Progress.IsIndeterminate = false;
            Progress.Minimum = 0;
            Progress.Maximum = 100;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            Message.Text = e.UserState as string;
        }

        public Working(Window c, string initialMessage, bool isIndeterminate, int min, int max): this(c)
        {
            Message.Text = initialMessage;
            Progress.IsIndeterminate = isIndeterminate;
            Progress.Minimum = min;
            Progress.Maximum = max;
        }

        private delegate void myDelegate();

        public void SetProgress(string message, int value)
        {
            Dispatcher.Invoke((myDelegate)delegate() { Progress.Value = value; Message.Text = message; });
        }

        public void SetRange(int min, int max)
        {
            Dispatcher.Invoke((myDelegate)delegate() { Progress.Minimum = min; Progress.Maximum = max; if (min < max) Progress.IsIndeterminate = false; });
        }

        public void CloseWindow()
        {
            Dispatcher.Invoke((myDelegate)delegate() { Close(); });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (worker != null)
            {
                worker.CancelAsync();
            }
        }
    }
}
