using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OodHelper.Data;
using OodHelper.Helpers;
using OodHelper.Services;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Boat.xaml
    /// </summary>
    public partial class BoatView : Window
    {
        public int Bid 
        {
            get 
            {
                BoatModel bm = DataContext as BoatModel;
                if (bm != null)
                    return bm.Bid.HasValue ? bm.Bid.Value : 0;
                return 0;
            } 
        }

        public BoatView(int b)
        {
            InitializeComponent();

            BoatModel bm = new BoatModel(b);
            DataContext = bm;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            //
            // If a user changes the content of a text box and then hits return
            // this trigger is fired without the text box firing it's lost focus
            // trigger, so we need to update the source for the text box by hand.
            //
            VisualHelper.UpdateTextBoxSources(this);

            var dc = DataContext as BoatModel;
            if (dc == null) return;

            string msg;
            if ((msg = dc.CommitChanges()) == string.Empty)
            {
                DialogResult = true;
                Close();
            }
            else
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Bid == 0)
            {
                int topseed = Settings.TopSeed;
                int nextval = App.Services.GetRequiredService<IBoatRepository>().GetNextIdentity();

                if (nextval > topseed)
                {
                    MessageBox.Show("You need to get a new set of seed values", "Cannot add a new boat",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;
                    this.Close();
                }
            }
        }

        private void SelectClass_Click(object sender, RoutedEventArgs e)
        {
            var id = App.Services.GetRequiredService<IDialogService>().ShowClassPicker();
            if (id == null) return;

            BoatModel dc = DataContext as BoatModel;
            if (dc == null) return;

            var pn = App.Services.GetRequiredService<IPortsmouthNumberRepository>().Get(id.Value);
            if (pn == null) return;

            dc.BoatClass = pn.ClassName;
            dc.OpenHandicap = pn.Number.ToString();
            if (dc.RollingHandicap == string.Empty)
                dc.RollingHandicap = pn.Number.ToString();
            switch (pn.Status)
            {
                case "P":
                    dc.HandicapStatus = "PY";
                    break;
                case "S":
                    dc.HandicapStatus = "SY";
                    break;
                case "C":
                    dc.HandicapStatus = "CN";
                    break;
                case "R":
                    dc.HandicapStatus = "RN";
                    break;
                case "E":
                    dc.HandicapStatus = "TN";
                    break;
            }

            dc.EnginePropeller = pn.Engine ?? "";

            switch (pn.Keel)
            {
                case "1":
                    dc.Keel = "F";
                    break;
                case "2":
                    dc.Keel = "2K";
                    break;
                case "3":
                    dc.Keel = "3K";
                    break;
            }
        }
    }
}
