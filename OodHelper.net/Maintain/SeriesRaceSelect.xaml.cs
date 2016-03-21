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

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for SeriesRaceSelect.xaml
    /// </summary>
    public partial class SeriesRaceSelect : Window
    {
        public int Sid { get; private set; }
        public SeriesRaceSelect(int sid)
        {
            InitializeComponent();
            Sid = sid;
            Hashtable p = new Hashtable();
            Db c = new Db("SELECT sid selected, calendar.rid, event, class as event_class, start_date " +
                "FROM calendar LEFT JOIN calendar_series_join ON calendar_series_join.rid = calendar.rid " +
                "AND calendar_series_join.sid = @sid " +
                "ORDER BY start_date");
            p["@sid"] = Sid;
            DataTable d = c.GetData(p);
            foreach (DataRow r in d.Rows)
                if (r["selected"] != DBNull.Value)
                    r["selected"] = true;
                else
                    r["selected"] = false;
            CalGrid.ItemsSource = d.AsDataView();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Hashtable p = new Hashtable();
            p["sid"] = Sid;
            Db c = new Db("DELETE FROM calendar_series_join WHERE sid = @sid");
            c.ExecuteNonQuery(p);
            c = new Db("INSERT INTO calendar_series_join (sid, rid) VALUES (@sid, @rid)");

            var _races = (CalGrid.ItemsSource as DataView).Table.AsEnumerable();

            foreach (DataRow r in (from r in _races
                where r.Field<int>("selected") == 1
                select r))
            {
                p["rid"] = r["rid"];
                c.ExecuteNonQuery(p);
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        System.Timers.Timer t = null;

        void Eventname_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (t == null)
                t = new System.Timers.Timer(500);
            else
                t.Stop();
            t.AutoReset = false;
            t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new dFilterRaces(FilterRaces), null);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        public delegate void dFilterRaces();

        public void FilterRaces()
        {
            ((DataView)CalGrid.ItemsSource).RowFilter =
                String.Format("event LIKE '%{0}%' OR event_class LIKE '%{0}%'", Eventname.Text);
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            DataView v = (DataView)CalGrid.ItemsSource;
            DataTable filt = v.ToTable();
            DataTable d = v.Table;

            foreach (DataRow r in filt.Rows)
            {
                DataRow[] rd = v.Table.Select(string.Format("rid = {0}", r["rid"]));
                foreach (DataRow u in rd)
                    u["selected"] = true;
            }
        }
    }
}
