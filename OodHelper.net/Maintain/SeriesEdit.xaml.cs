using System;
using System.Collections;
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

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for SeriesEdit.xaml
    /// </summary>
    public partial class SeriesEdit : Window, INotifyPropertyChanged
    {
        public int Sid { get; private set; }

        private string _sname;
        public string Sname
        {
            get
            {
                return _sname;
            }
            set
            {
                _sname = value;
                OnPropertyChanged("Sname");
            }
        }

        private string _discards;
        public string Discards
        {
            get
            {
                return _discards;
            }
            set
            {
                _discards = value;
                OnPropertyChanged("Discards");
            }
        }

        public SeriesEdit(int sid)
        {
            InitializeComponent();
            DataContext = this;
            Sid = sid;
            if (Sid != 0)
            {
                Hashtable p = new Hashtable();
                Db c = new Db("SELECT sname, discards FROM series WHERE sid = @sid");
                p["sid"] = Sid;
                Hashtable d = c.GetHashtable(p);
                Sname = d["sname"] as string;
                Discards = d["discards"] as string;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Db c;
            Hashtable p = new Hashtable();
            if (Sname == null || Sname.Trim() == string.Empty)
            {
                MessageBox.Show("Series name cannot be blank", "Validation error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            p["sname"] = Sname.Trim();
            p["discards"] = Discards.Trim() == string.Empty ? null : Discards;
            if (Sid == 0)
            {
                c = new Db("INSERT INTO series (sname, discards) VALUES (@sname, @discards)");
                Sid = c.GetNextIdentity("series", "sid");
            }
            else
            {
                c = new Db("UPDATE series SET sname = @sname, discards = @discards WHERE sid = @sid");
                p["sid"] = Sid;
            }
            c.ExecuteNonQuery(p);
            DialogResult = true;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
