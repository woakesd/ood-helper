using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OodHelper.net
{
    /// <summary>
    /// Interaction logic for RaceEdit.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class RaceEdit : UserControl
    {
        private Db rddb;
        private DataTable rd;
        private Hashtable caldata;

        public IRaceScore scorer;

        private string raceclass = "";
        public string RaceClass
        {
            get { return raceclass; }
        }

        private string racename = "";
        public string RaceName
        {
            get { return racename; }
        }

        private DateTime mRaceDate;
        public DateTime RaceDate
        {
            get { return mRaceDate; }
        }

        private string mOod;
        public string Ood
        {
            get { return mOod; }
        }

        public string RaceStart
        {
            get { return start.Text; }
        }

        private int rid;
        public int Rid { get { return rid; } }

        private string eventname;

        public RaceEdit(int r)
        {
            InitializeComponent();

            rid = r;

            LoadGrid();
        }

        private string mHandicap;
        public string Handicap
        {
            get
            {
                return mHandicap;
            }
        }

        private bool PreviousCompetitors(int rid)
        {
            return false;
        }

        public void LoadGrid()
        {
            Db c = new Db("SELECT start_date, time_limit_fixed, extension, event, class, timegate, handicapping, standard_corrected_time " +
                    "FROM calendar " +
                    "WHERE rid = @rid");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            caldata = c.GetHashtable(p);

            start.Text = (caldata["start_date"] as DateTime?).Value.TimeOfDay.ToString("hh\\:mm");
            if (caldata["time_limit_fixed"] != DBNull.Value)
                timeLimit.Text = (caldata["time_limit_fixed"] as DateTime?).Value.TimeOfDay.ToString("hh\\:mm");
            extension.Text = caldata["extension"].ToString();
            DateTime raceDate = (DateTime)caldata["start_date"];
            raceName.Content = raceDate.ToString("ddd") + " " +
                raceDate.ToString("dd MMM yyyy") +
                " (" + ((caldata["handicapping"].ToString() == "r") ? "Rolling " : "Open ") + "handicap)";
            eventname = caldata["event"].ToString().Trim();
            racename = eventname + " - " + caldata["class"].ToString().Trim();
            raceclass = caldata["class"].ToString().Trim();
            mRaceDate = (DateTime)caldata["start_date"];
            //mOod = caldata["ood"].ToString();
            if (caldata["handicapping"] != DBNull.Value)
                mHandicap = (string)caldata["handicapping"];
            sct.Text = Common.HMS((double)caldata["standard_corrected_time"]);

            if (scorer == null)
            {
                switch (Handicap)
                {
                    case "r":
                        scorer = new RollingHandicap();
                        break;
                    case "o":
                        scorer = new OpenHandicap();
                        break;
                }
            }

            rddb = new Db("SELECT r.rid, r.bid, boatname, boatclass, sailno, r.start_date, " +
                    "r.finish_code, r.finish_date, r.laps, r.override_points, r.elapsed, r.standard_corrected, r.corrected, r.place, " +
                    "r.points, r.open_handicap, r.rolling_handicap, r.achieved_handicap, " +
                    "r.new_rolling_handicap, r.handicap_status, r.c, r.a, r.performance_index " +
                    "FROM races r INNER JOIN boats ON boats.bid = r.bid " +
                    "WHERE r.rid = @rid " +
                    "ORDER BY place");
            rd = rddb.GetData(p);

            //
            // Set the columns which are to be editable as not being read only 
            // in the dataset.
            //
            foreach (DataColumn col in rd.Columns)
            {
                col.ReadOnly = true;
            }
            if ((bool)caldata["timegate"])
                rd.Columns["start_date"].ReadOnly = false;
            rd.Columns["finish_code"].ReadOnly = false;
            rd.Columns["finish_date"].ReadOnly = false;
            rd.Columns["laps"].ReadOnly = false;
            rd.Columns["override_points"].ReadOnly = false;

            Races.ItemsSource = rd.DefaultView;
        }

        void Races_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGridTextColumn col = (DataGridTextColumn)Races.Columns[rd.Columns["elapsed"].Ordinal];
            Binding b = (Binding)col.Binding;
            b.Converter = new IntTimeSpan();
            col.Binding = b;

            col = (DataGridTextColumn)Races.Columns[rd.Columns["standard_corrected"].Ordinal];
            b = (Binding)col.Binding;
            b.Converter = new DoubleTimeSpan();
            col.Binding = b;

            col = (DataGridTextColumn)Races.Columns[rd.Columns["corrected"].Ordinal];
            b = (Binding)col.Binding;
            b.Converter = new DoubleTimeSpan();
            col.Binding = b;

            col = (DataGridTextColumn)Races.Columns[rd.Columns["start_date"].Ordinal];
            col.Binding.StringFormat = "HH:mm:ss";

            col = (DataGridTextColumn)Races.Columns[rd.Columns["finish_date"].Ordinal];
            col.Binding.StringFormat = "HH:mm:ss";

            Color x = new Color();
            x.A = 255;
            x.R = 240;
            x.B = 240;
            x.G = 240;
            SolidColorBrush vlg = new SolidColorBrush(x);
            foreach (DataGridColumn c in Races.Columns)
            {
                if (rd.Columns[(string) c.Header].ReadOnly)
                {
                    c.CellStyle = new System.Windows.Style();
                    //c.CellStyle.BasedOn = App.Current.Resources.FindName("DataGridCellFont") as Style;
                    c.CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, vlg));
                    c.CellStyle.Setters.Add(new Setter(DataGridCell.ForegroundProperty, Brushes.Black));
                    c.CellStyle.Setters.Add(new Setter(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display));
                    //c.CellStyle.Setters.Add(new Setter(DataGridCell.FontSizeProperty, 14.0));
                }
            }

            Races.Columns[rd.Columns["rid"].Ordinal].Visibility = Visibility.Hidden;
            Races.Columns[rd.Columns["bid"].Ordinal].Visibility = Visibility.Hidden;
        }

        private void DataGridCell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                DataGridCell cell = sender as DataGridCell;
                if (cell != null)
                {
                    Races.CommitEdit();
                    cell.IsEditing = false;
                    DataGridRow row = FindVisualParent<DataGridRow>(cell);
                }
                e.Handled = false;
            }
        }

        void DataGridCell_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                DataGridCell cell = sender as DataGridCell;
                if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
                {
                    cell.Focus();
                    Races.BeginEdit();
                }
            }
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                preEdit = ((TextBlock)cell.Content).Text;
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }

        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
        
        private DataTable autoPopulateData = null;

        public int CountAutoPopulateData()
        {
            Db c = new Db("SELECT DISTINCT r.bid " +
                    "FROM calendar AS c1 " +
                    "INNER JOIN calendar_series_join AS cs1 ON c1.rid = cs1.rid " +
                    "INNER JOIN calendar_series_join AS cs2 ON cs2.sid = cs1.sid " +
                    "INNER JOIN calendar AS c2 ON c2.rid = cs2.rid AND c1.rid <> c2.rid AND c1.class = c2.class " +
                    "INNER JOIN races AS r ON r.rid = c2.rid " +
                    "WHERE c1.rid = @rid");
            Hashtable p = new Hashtable();
            p["rid"] = Rid;
            autoPopulateData = c.GetData(p);
            return autoPopulateData.Rows.Count;
        }

        public void DoAutoPopulate()
        {
            Db add = new Db(@"INSERT INTO races
                    (rid, start_date, bid, rolling_handicap, handicap_status, open_handicap, last_edit)
                    SELECT c.rid, c.start_date, b.bid, b.rolling_handicap, b.handicap_status, b.open_handicap, GETDATE()
                    FROM boats b, calendar c
                    WHERE b.bid = @bid
                    AND c.rid = @rid");
            Hashtable a = new Hashtable();
            a["rid"] = Rid;
            foreach (DataRow r in autoPopulateData.Rows)
            {
                a["bid"] = r["bid"];
                add.ExecuteNonQuery(a);
            }
            LoadGrid();
        }

        private string preEdit;

        void Races_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            TextBox te = e.EditingElement as TextBox;
            DataGridCell cell = te.Parent as DataGridCell;
            if (te != null)
            {
                DataRowView rowview = cell.DataContext as DataRowView;
                preEdit = rowview.Row[Races.Columns.IndexOf(cell.Column)].ToString();
                te.SelectAll();
            }
        }

        void Races_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Regex rxc = new Regex("^[a-z]{3}$", RegexOptions.IgnoreCase);
            Regex rxt = new Regex("^[0-9]{2}([: ][0-9]{2}){2}$");
            Regex rxl = new Regex("^[0-9]+$");
            if (((TextBox)e.EditingElement).Text != preEdit)
            {
                switch (e.Column.Header.ToString())
                {
                    case "finish_date":
                        TextBox fintime = (TextBox)e.EditingElement;
                        if (rxt.IsMatch(fintime.Text) || fintime.Text == string.Empty)
                        {
                            Db u = new Db(@"UPDATE races
                                SET finish_date = @fintime
                                , last_edit = GETDATE()
                                WHERE rid = @rid
                                AND bid = @bid");
                            Hashtable p = new Hashtable();
                            fintime.Text = fintime.Text.Replace(" ", ":");
                            if (fintime.Text == string.Empty)
                                p["fintime"] = DBNull.Value;
                            else
                                p["fintime"] = mRaceDate.Date + TimeSpan.Parse(fintime.Text);
                            p["bid"] = ((DataRowView)e.Row.Item).Row["bid"];
                            p["rid"] = Rid;
                            u.ExecuteNonQuery(p);
                        }
                        break;
                    case "finish_code":
                        TextBox fincode = (TextBox)e.EditingElement;
                        if (rxc.IsMatch(fincode.Text) || fincode.Text == string.Empty)
                        {
                            Db u = new Db(@"UPDATE races
                                SET finish_code = @fincode
                                , last_edit = GETDATE()
                                WHERE rid = @rid
                                AND bid = @bid");
                            Hashtable p = new Hashtable();
                            if (fincode.Text == string.Empty)
                                p["fintime"] = DBNull.Value;
                            else
                                p["fincode"] = fincode.Text.ToUpper();
                            p["bid"] = ((DataRowView)e.Row.Item).Row["bid"];
                            p["rid"] = Rid;
                            u.ExecuteNonQuery(p);
                        }
                        break;
                    case "laps":
                        TextBox laps = (TextBox)e.EditingElement;
                        if (laps.Text == string.Empty || rxl.IsMatch(laps.Text) && Int32.Parse(laps.Text) > 0)
                        {
                            Db u = new Db(@"UPDATE races
                                SET laps = @laps
                                , last_edit = GETDATE()
                                WHERE rid = @rid
                                AND bid = @bid");
                            Hashtable p = new Hashtable();
                            if (laps.Text == string.Empty)
                                p["laps"] = DBNull.Value;
                            else
                                p["laps"] = laps.Text;
                            p["bid"] = ((DataRowView)e.Row.Item).Row["bid"];
                            p["rid"] = Rid;
                            u.ExecuteNonQuery(p);
                        }
                        break;
                }
            }
        }

        void start_LostFocus(object sender, RoutedEventArgs e)
        {
            Regex rx = new Regex("^[0-9][0-9][: ][0-9][0-9]$");
            TimeSpan startTime;
            if (pcStart != start.Text && rx.IsMatch(start.Text) && TimeSpan.TryParse(start.Text, out startTime))
            {
                Db u = new Db(@"UPDATE races
                        SET start_date = @start_date
                        , last_edit = GETDATE()
                        WHERE rid = @rid");
                Hashtable p = new Hashtable();
                start.Text = start.Text.Replace(' ', ':');
                p["start_date"] = mRaceDate.Date + startTime;
                p["rid"] = Rid;
                u.ExecuteNonQuery(p);

                u = new Db(@"UPDATE calendar
                        SET start_date = @start_date
                        WHERE rid = @rid");
                u.ExecuteNonQuery(p);

                LoadGrid();
            }
            else
                start.Text = pcStart;
        }

        private string pcStart;

        void start_GotFocus(object sender, RoutedEventArgs e)
        {
            pcStart = start.Text;
        }

        private void buttonCalculate_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        public void Calculate()
        {
            if (scorer != null) scorer.Calculate(Rid);
            LoadGrid();
        }

        private void Races_CurrentCellChanged(object sender, EventArgs e)
        {
        }
    }
}
