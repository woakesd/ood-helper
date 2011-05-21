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
    [Svn("$Id$")]
    public partial class SeriesEdit : Window
    {
        class Data : NotifyPropertyChanged
        {
            public int Sid { get; set; }

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
        }

        Data dc = new Data();

        public SeriesEdit(int sid)
        {
            InitializeComponent();
            DataContext = dc;
            dc.Sid = sid;
            if (dc.Sid != 0)
            {
                Hashtable p = new Hashtable();
                Db c = new Db("SELECT sname, discards FROM series WHERE sid = @sid");
                p["sid"] = dc.Sid;
                Hashtable d = c.GetHashtable(p);
                dc.Sname = d["sname"] as string;
                dc.Discards = d["discards"] as string;
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
            if (dc.Sname == null || dc.Sname.Trim() == string.Empty)
            {
                MessageBox.Show("Series name cannot be blank", "Validation error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            p["sname"] = dc.Sname.Trim();
            p["discards"] = dc.Discards.Trim() == string.Empty ? null : dc.Discards;
            if (dc.Sid == 0)
            {
                c = new Db("INSERT INTO series (sname, discards) VALUES (@sname, @discards)");
                dc.Sid = c.GetNextIdentity("series", "sid");
            }
            else
            {
                c = new Db("UPDATE series SET sname = @sname, discards = @discards WHERE sid = @sid");
                p["sid"] = dc.Sid;
            }
            c.ExecuteNonQuery(p);
            DialogResult = true;
            Close();
        }
    }
}
