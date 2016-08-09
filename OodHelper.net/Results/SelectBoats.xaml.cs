using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OodHelper.Maintain;
using OodHelper.Rules;

namespace OodHelper.Results
{
    /// <summary>
    ///     Interaction logic for SelectBoats.xaml
    /// </summary>
    public partial class SelectBoats
    {
        public delegate void DFilterBoats();

        private readonly Hashtable _boatClasses;
        private readonly ResultsEditor[] _reds;
        private readonly BoatSelectRule[] _rules;
        private readonly SelectedBoats[] _sbt;

        private StringBuilder _boatsSql;
        private Timer _timer;

        public SelectBoats(ResultsEditor[] raceEdits)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            Width = SystemParameters.MaximizedPrimaryScreenWidth*0.8;
            Height = SystemParameters.MaximizedPrimaryScreenHeight*0.8;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _reds = raceEdits;
            _boatClasses = new Hashtable();

            _sbt = new SelectedBoats[_reds.Length];
            _rules = new BoatSelectRule[_reds.Length];

            for (int i = 0; i < _reds.Length; i++)
            {
                var ti = new TabItem {Header = _reds[i].RaceClass};

                _sbt[i] = new SelectedBoats(_reds[i].Rid);
                _boatClasses[_reds[i].RaceClass] = _sbt[i];

                _sbt[i].FleetName.Content = _reds[i].RaceName;

                ti.Content = _sbt[i];
                Fleets.Items.Add(ti);

                var d = _reds[i].Races.ItemsSource as IList<ResultModel>;
                var bts = new DataTable {TableName = "boats"};
                bts.Columns.Add(new DataColumn("bid", typeof (int)));
                var pk = new DataColumn[1];
                pk[0] = bts.Columns["bid"];
                bts.PrimaryKey = pk;
                bts.Columns.Add(new DataColumn("boatname"));
                bts.Columns.Add(new DataColumn("boatclass"));
                bts.Columns.Add(new DataColumn("sailno"));
                bts.Columns.Add(new DataColumn("handicap_status"));
                bts.Columns.Add(new DataColumn("open_handicap", typeof (int)));
                bts.Columns.Add(new DataColumn("rolling_handicap", typeof (int)));
                if (d != null)
                {
                    foreach (var r in d)
                    {
                        DataRow gr = bts.NewRow();
                        gr["bid"] = r.Bid;
                        gr["boatname"] = r.BoatName;
                        gr["boatclass"] = r.BoatClass;
                        gr["sailno"] = r.SailNo;
                        gr["handicap_status"] = r.HandicapStatus;
                        gr["open_handicap"] = r.OpenHandicap;
                        gr["rolling_handicap"] = r.RollingHandicap;
                        bts.Rows.Add(gr);
                    }
                }
                bts.AcceptChanges();
                _sbt[i].Boats.ItemsSource = bts.DefaultView;
                _sbt[i].Boats.IsReadOnly = true;
                _sbt[i].Boats.ContextMenu = new ContextMenu();

                _rules[i] = new BoatSelectRule(_reds[i].RaceClass);
            }

            for (int i = 0; i < _sbt.Length; i++)
            {
                for (int j = 0; j < _reds.Length; j++)
                {
                    if (j != i)
                    {
                        var m = new MenuItem
                        {
                            Header = "Move to " + _reds[j].RaceClass,
                            Command = new FleetChanger(_sbt[i].Boats, _sbt[j].Boats)
                        };
                        _sbt[i].Boats.ContextMenu.Items.Add(m);
                    }
                }
            }
        }

        private void Boatname_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_timer == null)
            {
                _timer = new Timer(500);
                _timer.Elapsed += TimerElapsed;
            }
            else
                _timer.Stop();
            _timer.AutoReset = false;
            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new DFilterBoats(FilterBoats), null);
        }

        public void FilterBoats()
        {
            if (Boatname.Text.Trim() != string.Empty)
            {
                _boatsSql =
                    new StringBuilder(
                        @"SELECT bid, boatname, boatclass, sailno, dinghy, handicap_status, open_handicap, rolling_handicap, firstname + ' ' + surname name
FROM boats
LEFT JOIN people ON boats.id = people.id ");
                bool yachts = false;
                bool dinghies = false;

                foreach (var t in _reds)
                {
                    if (t.RaceClass.IndexOf("Yacht", StringComparison.Ordinal) >= 0)
                        yachts = true;

                    if (t.RaceClass.IndexOf("Div", StringComparison.Ordinal) >= 0)
                        yachts = true;

                    if (t.RaceClass.IndexOf("Dinghy", StringComparison.Ordinal) >= 0)
                        dinghies = true;

                    if (t.RaceClass.IndexOf("All", StringComparison.Ordinal) >= 0)
                    {
                        yachts = true;
                        dinghies = true;
                    }
                }

                _boatsSql.Append(@"WHERE (boatname LIKE @filter
OR sailno LIKE @filter
OR boatclass LIKE @filter
OR firstname LIKE @filter
OR surname LIKE @filter) ");

                if (!dinghies && yachts)
                    _boatsSql.Append(@"AND dinghy = 0 ");
                else if (dinghies && !yachts)
                    _boatsSql.Append(@"AND dinghy = 1 ");

                _boatsSql.Append(@"ORDER BY boatname");

                var c = new Db(_boatsSql.ToString());
                var para = new Hashtable();
                para["filter"] = string.Format("%{0}%", Boatname.Text.Trim());
                var dt = c.GetData(para);

                Boats.ItemsSource = dt.DefaultView;
                Boats.IsReadOnly = true;
            }
            else
                Boats.ItemsSource = null;
        }

        private void Boats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddBoats();
        }

        private void AddBoats()
        {
            if (Boats.Items.Count == 1)
                Boats.SelectAll();
            var x = Boats.SelectedItems;
            if (x.Count == 0)
            {
                MessageBox.Show("No boats are highlighted (hint: click on the boat name)", "Hint", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                foreach (DataRowView rv in x)
                {
                    var alreadySelected = false;
                    foreach (SelectedBoats selboat in _sbt)
                    {
                        if (((DataView) selboat.Boats.ItemsSource).Table.Select("bid = " + rv["bid"]).Length > 0)
                        {
                            alreadySelected = true;
                        }
                    }

                    if (alreadySelected) continue;
                    
                    var choices = _rules.Where(t => t.AppliesToBoat(rv)).ToList();

                    if (choices.Count == 1)
                        AddBoat((SelectedBoats) _boatClasses[choices[0].Name], rv);
                    else if (choices.Count > 1)
                    {
                        var rs = new RuleSelector(choices);
                        var val = rs.ShowDialog();
                        if (!val.HasValue || !val.Value) continue;

                        var rc = rs.RuleChoice.SelectedItem as BoatSelectRule;
                        if (rc != null) AddBoat((SelectedBoats) _boatClasses[rc.Name], rv);
                    }
                    else
                    {
                        if (_rules.Count() > 1)
                        {
                            var rs = new RuleSelector(new List<BoatSelectRule>(_rules));
                            var val = rs.ShowDialog();
                            if (!val.HasValue || !val.Value) continue;

                            var rc = rs.RuleChoice.SelectedItem as BoatSelectRule;
                            if (rc != null) AddBoat((SelectedBoats) _boatClasses[rc.Name], rv);
                        }
                        else
                        {
                            AddBoat((SelectedBoats) _boatClasses[_rules.First().Name], rv);
                        }
                    }
                }
            }
        }

        private void AddBoat(SelectedBoats sbt, DataRowView rv)
        {
            DataRow dr = ((DataView) sbt.Boats.ItemsSource).Table.NewRow();
            dr["bid"] = rv["bid"];
            dr["boatname"] = rv["boatname"];
            dr["boatclass"] = rv["boatclass"];
            dr["sailno"] = rv["sailno"];
            dr["handicap_status"] = rv["handicap_status"];
            dr["open_handicap"] = rv["open_handicap"];
            dr["rolling_handicap"] = rv["rolling_handicap"];
            ((DataView) sbt.Boats.ItemsSource).Table.Rows.Add(dr);
            ((DataView) sbt.Boats.ItemsSource).Table.AcceptChanges();
            ((DataView) sbt.Boats.ItemsSource).Sort = "Boatname";
            Notify.Text = string.Format("Added {0} to {1}", new[] {dr["boatname"], sbt.FleetName.Content});
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var delete = new Db("DELETE FROM races WHERE rid = @rid AND bid = @bid");
            var add = new Db(@"INSERT INTO races
                    (rid, bid, start_date, handicap_status, open_handicap, rolling_handicap, last_edit)
                    VALUES (@rid, @bid, @start_date, @handicap_status, @open_handicap, @rolling_handicap, GETDATE())");
            var a = new Hashtable();

            DialogResult = true;
            for (var i = 0; i < _sbt.Length; i++)
            {
                var resultModels = _reds[i].Races.ItemsSource as IList<ResultModel>;
                a["rid"] = _sbt[i].RaceId;
                DataTable sb = ((DataView) _sbt[i].Boats.ItemsSource).Table;
                var selectedBids = new Hashtable();
                //
                // For each boat in selected boats check to see if it is in the race edit control,
                // if not then add it.
                //
                foreach (DataRow r in sb.Rows)
                {
                    if (resultModels != null &&
                        !resultModels.Any(rm => rm.Bid == (int) r["bid"] && rm.Rid == (int) a["rid"]))
                    {
                        a["bid"] = r["bid"];
                        a["start_date"] = _reds[i].StartDate;
                        a["handicap_status"] = r["handicap_status"];
                        a["open_handicap"] = r["open_handicap"];
                        a["rolling_handicap"] = r["rolling_handicap"];
                        add.ExecuteNonQuery(a);
                    }
                    selectedBids[r["bid"]] = true;
                }
                
                //
                // For each boat in the race edit control check to see if it is in selected boats,
                // if not then delete it.
                //
                if (resultModels == null) continue;
                
                foreach (var r in resultModels)
                {
                    a["bid"] = r.Bid;
                    if (!selectedBids.ContainsKey(r.Bid))
                    {
                        delete.ExecuteNonQuery(a);
                    }
                }
            }
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NewBoat_Click(object sender, RoutedEventArgs e)
        {
            var b = new BoatView(0) {Owner = this};
            if (b.ShowDialog() != true) return;
            
            var bm = b.DataContext as BoatModel;
            if (bm != null)
            {
                Boatname.Text = bm.BoatName;
                FilterBoats();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F3:
                    Boatname.Focus();
                    break;
                case Key.F4:
                    AddBoats();
                    break;
            }
        }

        private void SelectBoat_Click(object sender, RoutedEventArgs e)
        {
            AddBoats();
        }

        private void DeselectBoat_Click(object sender, RoutedEventArgs e)
        {
            var fleet = Fleets.SelectedItem as TabItem;
            if (fleet != null)
            {
                var sb = fleet.Content as SelectedBoats;
                if (sb != null)
                {
                    sb.RemoveBoats();
                }
            }
        }

        private class FleetChanger : ICommand
        {
            private readonly DataGrid _toGrid;
            private readonly DataGrid _fromGrid;

            public FleetChanger(DataGrid from, DataGrid to)
            {
                _fromGrid = from;
                _toGrid = to;
            }

            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return true;
            }

            // disable unused event warning
#pragma warning disable 67
            public event EventHandler CanExecuteChanged;
#pragma warning restore 67

            public void Execute(object parameter)
            {
                foreach (var fleet in _fromGrid.SelectedItems)
                {
                    var drv = fleet as DataRowView;
                    var toTable = ((DataView) _toGrid.ItemsSource).Table;
                    var n = toTable.NewRow();
                    if (drv != null)
                    {
                        for (var i = 0; i < drv.Row.ItemArray.Length; i++)
                            n[i] = drv.Row[i];
                    }
                    toTable.Rows.Add(n);
                    toTable.AcceptChanges();
                }
                while (_fromGrid.SelectedItems.Count > 0)
                {
                    var drv = _fromGrid.SelectedItems[0] as DataRowView;
                    if (drv == null) continue;
                    drv.DataView.Table.Rows.Remove(drv.Row);
                    drv.DataView.Table.AcceptChanges();
                }
            }

            #endregion
        }
    }
}