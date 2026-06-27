using System;
using System.Windows.Controls;
using OodHelper.Results;

namespace OodHelper
{
    /// <summary>
    /// Print page for one series division. Binds to <see cref="SeriesDisplayViewModel"/> (same rows
    /// and generated R1..Rn columns as the on-screen <see cref="SeriesDisplay"/>); the title and
    /// printed-on stamp are set here.
    /// </summary>
    public partial class SeriesDisplayPage : Page
    {
        public SeriesDisplayPage(SeriesDisplayViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            EventDescription.Text = viewModel.SeriesName;
            PrintedDate.Text = string.Format("Printed on {0:dd MMM yyyy} at {0:HH:mm:ss}", DateTime.Now);
        }
    }
}
