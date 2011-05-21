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

namespace OodHelper.Membership
{
    /// <summary>
    /// Interaction logic for Cards.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class Card : UserControl
    {
        public string MemberName { get; set; }
        public int MemberNumber { get; set; }
        public int FamilyNumber { get; set; }
        public Visibility FamilyVisible { get; set; }
        public string MembershipClass { get; set; }

        public Card(string memberName, int memberNumber, int familyNumber, string membershipClass)
        {
            InitializeComponent();
            MemberName = memberName;
            MemberNumber = memberNumber;
            FamilyNumber = familyNumber;
            if (FamilyNumber == MemberNumber)
                FamilyVisible = Visibility.Collapsed;
            else
                FamilyVisible = Visibility.Visible;
            MembershipClass = membershipClass;

            DataContext = this;
        }

        public Card()
        {
            InitializeComponent();
            FamilyVisible = Visibility.Visible;
            DataContext = this;
            //TMemberName.Text = "".PadRight(58, '_');
            TMemberName.Background = new SolidColorBrush(Colors.White);
            //TMemberNumber.Text = "".PadRight(4, '_');
            TMemberNumber.Background = new SolidColorBrush(Colors.White);
            //TFamilyNumber.Text = "".PadRight(4, '_');
            TFamilyNumber.Background = new SolidColorBrush(Colors.White);
            //TMembershipClass.Text = "".PadRight(6, '_');
            TMembershipClass.Background = new SolidColorBrush(Colors.White);
        }
    }
}
