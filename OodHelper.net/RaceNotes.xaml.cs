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

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for RaceNotes.xaml
    /// </summary>
    public partial class RaceNotes : Window
    {
        private int Rid { get; set; }
        public RaceNotes(int rid)
        {
            Rid = rid;
            InitializeComponent();
            Db c = new Db(@"SELECT event, class, memo
                    FROM calendar WHERE rid = @rid");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            Hashtable d = c.GetHashtable(p);
            Event.Text = d["event"] as string;
            Class.Text = d["class"] as string;
            Memo.Text = d["memo"] as string;
            c.Dispose();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Db c = new Db(@"UPDATE calendar
                    SET memo = @memo WHERE rid = @rid");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            p["memo"] = Memo.Text;
            c.ExecuteNonQuery(p);
            c.Dispose();
            DialogResult = true;
            Close();
        }
    }
}
