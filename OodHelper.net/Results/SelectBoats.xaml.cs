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
using Microsoft.Extensions.DependencyInjection;
using OodHelper.Data;
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
        private readonly IBoatRepository _boatRepo;
        private readonly IRaceResultsRepository _raceRepo;

        private Timer? _timer;

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

            // This window is not constructed through DI, so its repositories are resolved from the
            // container here. Boat search, the rule trees and race-entry writes all go via EF.
            var ruleRepo = App.Services.GetRequiredService<ISelectRuleRepository>();
            _boatRepo = App.Services.GetRequiredService<IBoatRepository>();
            _raceRepo = App.Services.GetRequiredService<IRaceResultsRepository>();

            for (int i = 0; i < _reds.Length; i++)
            {
                var ti = new TabItem {Header = _reds[i].RaceClass};

                _sbt[i] = new SelectedBoats(_reds[i].Rid);
                _boatClasses[_reds[i].RaceClass] = _sbt[i];

                _sbt[i].FleetName.Content = _reds[i].RaceName;

                ti.Content = _sbt[i];
                Fleets.Items.Add(ti);

                IReadOnlyList<ResultRowViewModel> d = _reds[i].Rows;
                var bts = new DataTable {TableName = "boats"};
                bts.Columns.Add(new DataColumn("Bid", typeof (int)));
                var pk = new DataColumn[1];
                pk[0] = bts.Columns["Bid"]!;
                bts.PrimaryKey = pk;
                bts.Columns.Add(new DataColumn("BoatName"));
                bts.Columns.Add(new DataColumn("BoatClass"));
                bts.Columns.Add(new DataColumn("SailNo"));
                bts.Columns.Add(new DataColumn("HandicapStatus"));
                bts.Columns.Add(new DataColumn("OpenHandicap", typeof (int)));
                bts.Columns.Add(new DataColumn("RollingHandicap", typeof (int)));
                if (d != null)
                {
                    foreach (var r in d)
                    {
                        DataRow gr = bts.NewRow();
                        gr["Bid"] = r.Bid;
                        gr["BoatName"] = r.BoatName;
                        gr["BoatClass"] = r.BoatClass;
                        gr["SailNo"] = r.SailNo;
                        gr["HandicapStatus"] = r.HandicapStatus;
                        gr["OpenHandicap"] = r.OpenHandicap;
                        gr["RollingHandicap"] = r.RollingHandicap;
                        bts.Rows.Add(gr);
                    }
                }
                bts.AcceptChanges();
                _sbt[i].Boats.ItemsSource = bts.DefaultView;
                _sbt[i].Boats.IsReadOnly = true;
                _sbt[i].Boats.ContextMenu = new ContextMenu();

                _rules[i] = ruleRepo.GetTree(_reds[i].RaceClass);
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

        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new DFilterBoats(FilterBoats), null);
        }

        public void FilterBoats()
        {
            if (Boatname.Text.Trim() != string.Empty)
            {
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

                //
                // Restrict to yachts (dinghy = false) or dinghies (dinghy = true) when exactly one
                // applies; otherwise no dinghy filter, matching the old SQL.
                //
                bool? dinghyFilter = null;
                if (!dinghies && yachts) dinghyFilter = false;
                else if (dinghies && !yachts) dinghyFilter = true;

                var dt = BuildBoatTable(_boatRepo.Search(Boatname.Text.Trim(), dinghyFilter));

                Boats.ItemsSource = dt.DefaultView;
                Boats.IsReadOnly = true;
            }
            else
                Boats.ItemsSource = null;
        }

        private static DataTable BuildBoatTable(IReadOnlyList<Data.Entities.Boat> boats)
        {
            //
            // Columns are named for the Boat properties they carry, not for database columns: the rules
            // in BoatSelectRule.Fields index rows of this table by name, so every field a rule can test
            // (BoatClass, Dinghy, OpenHandicap, HullType) has to appear here.
            //
            var dt = new DataTable {TableName = "boats"};
            dt.Columns.Add(new DataColumn("Bid", typeof (int)));
            dt.Columns.Add(new DataColumn("BoatName"));
            dt.Columns.Add(new DataColumn("BoatClass"));
            dt.Columns.Add(new DataColumn("SailNo"));
            dt.Columns.Add(new DataColumn("Dinghy", typeof (bool)));
            dt.Columns.Add(new DataColumn("HullType"));
            dt.Columns.Add(new DataColumn("HandicapStatus"));
            dt.Columns.Add(new DataColumn("OpenHandicap", typeof (int)));
            dt.Columns.Add(new DataColumn("RollingHandicap", typeof (int)));
            foreach (var b in boats)
            {
                var r = dt.NewRow();
                r["Bid"] = b.Bid;
                r["BoatName"] = (object?) b.BoatName ?? DBNull.Value;
                r["BoatClass"] = (object?) b.BoatClass ?? DBNull.Value;
                r["SailNo"] = (object?) b.SailNo ?? DBNull.Value;
                r["Dinghy"] = (object?) b.Dinghy ?? DBNull.Value;
                r["HullType"] = (object?) b.HullType ?? DBNull.Value;
                r["HandicapStatus"] = (object?) b.HandicapStatus ?? DBNull.Value;
                r["OpenHandicap"] = (object?) b.OpenHandicap ?? DBNull.Value;
                r["RollingHandicap"] = (object?) b.RollingHandicap ?? DBNull.Value;
                dt.Rows.Add(r);
            }
            dt.AcceptChanges();
            return dt;
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
                        if (((DataView) selboat.Boats.ItemsSource!).Table!.Select("Bid = " + rv["Bid"]).Length > 0)
                        {
                            alreadySelected = true;
                        }
                    }

                    if (alreadySelected) continue;
                    
                    var choices = _rules.Where(t => t.AppliesToBoat(rv)).ToList();

                    if (choices.Count == 1)
                        AddBoat((SelectedBoats) _boatClasses[choices[0].Name!]!, rv);
                    else if (choices.Count > 1)
                    {
                        var rs = new RuleSelector(choices);
                        var val = rs.ShowDialog();
                        if (!val.HasValue || !val.Value) continue;

                        var rc = rs.RuleChoice.SelectedItem as BoatSelectRule;
                        if (rc != null) AddBoat((SelectedBoats) _boatClasses[rc.Name!]!, rv);
                    }
                    else
                    {
                        if (_rules.Count() > 1)
                        {
                            var rs = new RuleSelector(new List<BoatSelectRule>(_rules));
                            var val = rs.ShowDialog();
                            if (!val.HasValue || !val.Value) continue;

                            var rc = rs.RuleChoice.SelectedItem as BoatSelectRule;
                            if (rc != null) AddBoat((SelectedBoats) _boatClasses[rc.Name!]!, rv);
                        }
                        else
                        {
                            AddBoat((SelectedBoats) _boatClasses[_rules.First().Name!]!, rv);
                        }
                    }
                }
            }
        }

        private void AddBoat(SelectedBoats sbt, DataRowView rv)
        {
            DataRow dr = ((DataView) sbt.Boats.ItemsSource!).Table!.NewRow();
            dr["Bid"] = rv["Bid"];
            dr["BoatName"] = rv["BoatName"];
            dr["BoatClass"] = rv["BoatClass"];
            dr["SailNo"] = rv["SailNo"];
            dr["HandicapStatus"] = rv["HandicapStatus"];
            dr["OpenHandicap"] = rv["OpenHandicap"];
            dr["RollingHandicap"] = rv["RollingHandicap"];
            ((DataView) sbt.Boats.ItemsSource).Table!.Rows.Add(dr);
            ((DataView) sbt.Boats.ItemsSource).Table!.AcceptChanges();
            ((DataView) sbt.Boats.ItemsSource).Sort = "BoatName";
            Notify.Text = string.Format("Added {0} to {1}", new[] {dr["BoatName"], sbt.FleetName.Content});
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            for (var i = 0; i < _sbt.Length; i++)
            {
                IReadOnlyList<ResultRowViewModel> resultModels = _reds[i].Rows;
                int rid = _sbt[i].RaceId;
                DataTable sb = ((DataView) _sbt[i].Boats.ItemsSource!).Table!;
                var selectedBids = new Hashtable();
                //
                // For each boat in selected boats check to see if it is in the race edit control,
                // if not then add it.
                //
                foreach (DataRow r in sb.Rows)
                {
                    int bid = (int) r["Bid"];
                    if (resultModels != null &&
                        !resultModels.Any(rm => rm.Bid == bid && rm.Rid == rid))
                    {
                        _raceRepo.AddRaceEntry(rid, bid, _reds[i].StartDate,
                            r["HandicapStatus"] as string,
                            r["OpenHandicap"] == DBNull.Value ? (int?) null : (int) r["OpenHandicap"],
                            r["RollingHandicap"] == DBNull.Value ? (int?) null : (int) r["RollingHandicap"]);
                    }
                    selectedBids[bid] = true;
                }

                //
                // For each boat in the race edit control check to see if it is in selected boats,
                // if not then delete it.
                //
                if (resultModels == null) continue;

                foreach (var r in resultModels)
                {
                    if (!selectedBids.ContainsKey(r.Bid))
                    {
                        _raceRepo.DeleteRaceEntry(rid, r.Bid);
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

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            // disable unused event warning
#pragma warning disable 67
            public event EventHandler? CanExecuteChanged;
#pragma warning restore 67

            public void Execute(object? parameter)
            {
                foreach (var fleet in _fromGrid.SelectedItems)
                {
                    var drv = fleet as DataRowView;
                    var toTable = ((DataView) _toGrid.ItemsSource!).Table!;
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
                    drv.DataView.Table!.Rows.Remove(drv.Row);
                    drv.DataView.Table!.AcceptChanges();
                }
            }

            #endregion
        }
    }
}