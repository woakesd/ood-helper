using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OodHelper.Helpers;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for FamilyMember.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class FamilyMember : Window
    {
        public int Id
        {
            get
            {
                PersonModel m = DataContext as PersonModel;
                if (m != null)
                    return m.Id.HasValue ? m.Id.Value : 0;
                return 0;
            }
        }

        public int MainId
        {
            get {
                PersonModel m = DataContext as PersonModel;
                if (m != null)
                    return m.Id.HasValue ? m.Id.Value : 0;
                return 0;
            }
        }

        public FamilyMember(int id, int mainid)
        {
            InitializeComponent();

            PersonModel pm = new PersonModel(id, mainid);
            DataContext = pm;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            VisualHelper.UpdateTextBoxSources(this);

            PersonModel dc = DataContext as PersonModel;
            if (dc != null)
            {
                string msg;
                if ((msg = dc.CommitChanges()) == string.Empty)
                {
                    DialogResult = true;
                    Close();
                }
                else
                    MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            SelectCrewBoats d = new SelectCrewBoats(Id);
            if (d.ShowDialog() == true)
                ; // Crewing.ItemsSource = PersonView.BoatCrewFill(Id);
        }
    }
}
