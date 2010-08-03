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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for EntrySheets.xaml
    /// </summary>
    public partial class EntrySheet : Page
    {
        public EntrySheet()
        {
            InitializeComponent();
            string[] row = new string[] { "", "", "", "", "", "", "", "", "", "" };
            for (int i = 0; i < 15; i++)
                Entries.Items.Add(row);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
