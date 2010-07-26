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
    /// Interaction logic for Handicap.xaml
    /// </summary>
    public partial class Handicap : Window
    {
        public int Id { get; set; }
        HandicapRecord hcap;
        HandicapLinq hl = new HandicapLinq();

        public Handicap(int i)
        {
            InitializeComponent();
            Id = i;
            if (Id != 0)
            {
                hcap = hl.portsmouth_numbers.Single(c => c.id == Id);
            }
            else
            {
                hcap = new HandicapRecord();
            }
            DataContext = hcap;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Id == 0)
                    hl.portsmouth_numbers.InsertOnSubmit(hcap);
                hl.SubmitChanges();
                Id = hcap.id;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
