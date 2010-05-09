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

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for Seed.xaml
    /// </summary>
    public partial class Seed : Window
    {
        public Seed()
        {
            InitializeComponent();
            Object o;
            if ((o = DbSettings.GetSetting("bottomseed")) != null)
                BottomSeed.Text = o.ToString();
            else
                BottomSeed.Text = "1";
            if ((o = DbSettings.GetSetting("topseed")) != null)
                TopSeed.Text = o.ToString();
            else
                TopSeed.Text = "1999";
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            int b = 0, t = 0;
            if (Int32.TryParse(BottomSeed.Text, out b) && b != 0)
            {
                DbSettings.AddSetting("bottomseed", b);
            }
            if (Int32.TryParse(TopSeed.Text, out t) && t != 0)
            {
                DbSettings.AddSetting("topseed", t);
            }
            ReseedDatabase();
            this.DialogResult = true;
            this.Close();
        }

        public static void ReseedDatabase()
        {
            Object o;
            int b, t;
            if ((o = DbSettings.GetSetting("bottomseed")) != null)
                b = (int) o;
            else
                b = 1;
            if ((o = DbSettings.GetSetting("topseed")) != null)
                t = (int) o;
            else
                t = 1999;

            ReseedTable("boats", "bid", b, t);
            ReseedTable("people", "id", b, t);
            ReseedTable("calendar", "rid");
            ReseedTable("series", "sid");
        }

        private static void ReseedTable(string tname, string ident, int b, int t)
        {
            if (b < t && b != 0 && t != 0)
            {
                Db s = new Db("SELECT MAX(" + ident + ") " +
                    "FROM " + tname + " " +
                    "WHERE " + ident + " BETWEEN @b AND @t");
                Hashtable p = new Hashtable();
                p["b"] = b;
                p["t"] = t;
                object o;
                int seedvalue;
                if ((o = s.GetScalar(p)) != null)
                {
                    seedvalue = ((int)o) + 1;
                }
                else
                {
                    seedvalue = b;
                }
                s = new Db("ALTER TABLE " + tname + " " +
                    "ALTER COLUMN " + ident + " IDENTITY(" + seedvalue.ToString() + ",1)");
                s.ExecuteNonQuery(null);
            }
        }

        private static void ReseedTable(string tname, string ident)
        {
            Db s = new Db("SELECT MAX(" + ident + ") " +
                "FROM " + tname);
            object o;
            int seedvalue;
            if ((o = s.GetScalar(null)) != null)
            {
                seedvalue = ((int)o) + 1;
            }
            else
            {
                seedvalue = 1;
            }
            s = new Db("ALTER TABLE " + tname + " " +
                "ALTER COLUMN " + ident + " IDENTITY(" + seedvalue.ToString() + ",1)");
            s.ExecuteNonQuery(null);
        }
    }
}
