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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Id == 0)
            {
                object o = DbSettings.GetSetting("topseed");
                if (o != null)
                {
                    int topseed, nextval;
                    topseed = (int)o;

                    Db seed = new Db(string.Empty);
                    nextval = seed.GetNextIdentity("people", "id");

                    if (nextval > topseed)
                    {
                        MessageBox.Show("You need to get a new set of seed values", "Cannot add a new person",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        this.DialogResult = false;
                        this.Close();
                    }
                }
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
            pm.Membership = "Fmemb";
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
            {
                PersonModel p = DataContext as PersonModel;
                if (p != null)
                    p.OnPropertyChanged("Crewing");
            }
        }
    }
}
