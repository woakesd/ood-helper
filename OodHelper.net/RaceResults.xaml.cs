using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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
    /// Interaction logic for RaceResults.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class RaceResults : UserControl
    {
        private RaceEdit[] reds;

        public RaceResults(int[] rids)
        {
            InitializeComponent();

            reds = new RaceEdit[rids.Length];

            bool askAutoPopulate = true, doAutoPopulate = false;
            for (int i = 0; i < rids.Length; i++)
            {
                TabItem t = new TabItem();
                RaceEdit r = new RaceEdit(rids[i]);
                if (!r.Races.HasItems && r.CountAutoPopulateData() > 0)
                {
                    if (askAutoPopulate)
                    {
                        askAutoPopulate = false;
                        if (MessageBox.Show("Would you like to copy competitors from previous race?",
                            "Auto populate", MessageBoxButton.YesNo, 
                            MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                        {
                            doAutoPopulate = true;
                        }
                    }
                    if (doAutoPopulate)
                    {
                        r.DoAutoPopulate();
                    }
                }
                reds[i] = r;
                t.Header = r.RaceName;
                t.Content = r;
                raceTabControl.Items.Add(t);
                if (rids.Length > 1)
                {
                    r.ContextMenu = new ContextMenu();
                }
            }

            if (rids.Length > 1)
            {
                for (int i = 0; i < rids.Length; i++)
                {
                    RaceEdit from = (RaceEdit)((TabItem)raceTabControl.Items[i]).Content;
                    ContextMenu m = from.ContextMenu;
                    MenuItem editBoat = new MenuItem();
                    editBoat.Header = "Edit Boat";
                    editBoat.Command = new EditBoat();
                    editBoat.CommandParameter = from;
                    m.Items.Add(editBoat);
                    for (int j = 0; j < rids.Length; j++)
                    {
                        RaceEdit to = (RaceEdit)((TabItem)raceTabControl.Items[j]).Content;
                        if (i != j)
                        {
                            MenuItem mi = new MenuItem();
                            mi.Header = "Move to " + to.RaceName;
                            mi.Command = new FleetChanger(this, from.Rid, to.Rid);
                            mi.CommandParameter = from.Races;
                            m.Items.Add(mi);
                        }
                    }
                }
            }
        }

        class EditBoat : ICommand
        {
            public EditBoat()
            {
            }

            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                bool reload = false;
                RaceEdit rr = (RaceEdit)parameter;
                foreach (DataRowView drv in rr.Races.SelectedItems)
                {
                    int bid = (int)drv.Row["bid"];
                    Boat edit = new Boat(bid);
                    if (edit.ShowDialog().Value)
                    {
                        Db c = new Db("SELECT bid, rolling_handicap, handicap_status, open_handicap " +
                            "FROM boats WHERE bid = @bid");
                        Hashtable p = new Hashtable();
                        p["bid"] = bid;
                        Hashtable d = c.GetHashtable(p);
                        foreach (object o in d.Keys)
                            p[o] = d[o];
                        p["rid"] = rr.Rid;
                        c = new Db("UPDATE races " +
                                "SET rolling_handicap = @rolling_handicap, " +
                                "handicap_status = @handicap_status, " +
                                "open_handicap = @open_handicap " +
                                "WHERE rid = @rid " +
                                "AND bid = @bid");
                        c.ExecuteNonQuery(p);
                        reload = true;
                    }
                }
                if (reload) rr.LoadGrid();
            }

            #endregion
        }

        class FleetChanger : ICommand
        {
            private int toRid;
            private int fromRid;
            private RaceResults rr;
            public FleetChanger(RaceResults r, int from, int to)
            {
                fromRid = from;
                toRid = to;
                rr = r;
            }

            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                DataGrid races = (DataGrid)parameter;
                if (races.SelectedItems.Count > 0)
                {
                    Db s = new Db(@"SELECT start
                            FROM calendar
                            WHERE rid = @torid");
                    Hashtable p = new Hashtable();
                    p["torid"] = toRid;
                    p["start"] = s.GetScalar(p);
                    Db c = new Db(@"UPDATE races
                            SET rid = @torid
                            , start = @start
                            WHERE rid = @fromrid
                            AND bid = @bid");
                    p["fromrid"] = fromRid;
                    p["start"] = p["start"] + ":00";
                    foreach (DataRowView drv in races.SelectedItems)
                    {
                        p["bid"] = drv.Row["bid"];
                        c.ExecuteNonQuery(p);
                    }

                    for (int i = 0; i < rr.reds.Length; i++)
                        rr.reds[i].LoadGrid();
                }
            }

            #endregion
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.PrintPreviewDialog pv = new System.Windows.Forms.PrintPreviewDialog();
            PrintRaces = new System.Drawing.Printing.PrintDocument();
            pv.Document = PrintRaces;
            PrintRaces.BeginPrint += new System.Drawing.Printing.PrintEventHandler(pd_BeginPrint);
            PrintRaces.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(pd_PrintPage);
            pv.ShowDialog();
        }

        System.Drawing.Printing.PrintDocument PrintRaces;
        private int PageNo;

        void pd_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //PrintRaces.PrinterSettings.PrinterName = "HP Photosmart C6180";

            foreach (RaceEdit rac in reds)
            {
                rac.Calculate();
            }
            PageNo = 0;
        }

        void pd_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            RaceEdit race = reds[PageNo];
            Font lfnt = new Font("Lucida Console", 10);
            Font lbfnt = new Font("Lucida Console", 10, System.Drawing.FontStyle.Bold);
            Font sfnt = new Font("MS Gothic", (float)8);
            System.Drawing.Brush b = System.Drawing.Brushes.Black;

            //PrintRaces.OriginAtMargins = true;

            string outs = "Port Edgar Yacht Club Race Results";
            RectangleF ps = PrintRaces.DefaultPageSettings.PrintableArea;
            //ps.Offset(50, 50)
            //ps.Inflate(-50, -50)
            int tmargin = 30;
            int lmargin = 50;

            SizeF tsize = e.Graphics.MeasureString(outs, lfnt);

            float ptop = ps.Top + tmargin;
            float pleft = ps.Left + lmargin;

            e.Graphics.DrawString(outs, lbfnt, b, (ps.Width - tsize.Width) / 2 + ps.X, ptop - tsize.Height);

            outs = "Date of Race: " + race.RaceDate.ToShortDateString();
            tsize = e.Graphics.MeasureString(outs, lfnt);
            e.Graphics.DrawString(outs, lfnt, b, (float)(15 + pleft), (float)(ptop + tsize.Height * 0.5));

            outs = "OOD: " + race.Ood;
            tsize = e.Graphics.MeasureString(outs, lfnt);
            e.Graphics.DrawString(outs, lfnt, b, (float)(ps.Width - lmargin - tsize.Width - 15), (float)(ptop + tsize.Height * 0.5));

            outs = "Race No: " + race.Rid.ToString();
            tsize = e.Graphics.MeasureString(outs, lfnt);
            e.Graphics.DrawString(outs, lfnt, b, 15 + pleft, ptop + tsize.Height * 2);

            outs = race.RaceName + " - " + race.RaceClass;
            tsize = e.Graphics.MeasureString(outs, lbfnt);
            e.Graphics.DrawString(outs, lbfnt, b, (ps.Width - tsize.Width) / 2, ptop + tsize.Height * 2);

            outs = "Start: " + race.RaceStart;
            tsize = e.Graphics.MeasureString(outs, lfnt);
            e.Graphics.DrawString(outs, lfnt, b, (float)(15 + pleft), (float)(ptop + tsize.Height * 3.5));

            if (race.Handicap == "r")
                outs = "Rolling Handicap Used";
            else
                outs = "Open Handicap Used";
            tsize = e.Graphics.MeasureString(outs, lfnt);
            e.Graphics.DrawString(outs, lfnt, b, (float)((ps.Width - tsize.Width) / 2), (float)(ptop + tsize.Height * 3.5));

            //outs = "SCT = " & Common.hmsh(race.SCT)
            //tsize = e.Graphics.MeasureString(outs, lfnt)
            //e.Graphics.DrawString(outs, lfnt, b, ps.Width - lmargin - tsize.Width - 15, ptop + tsize.Height * 3.5)

            float resoff = ptop + tsize.Height * 5;

            outs = "Boatname            " +
                "Boatclass          " +
                "Sail No  " +
                "HCap " +
                " Finish   " +
                "Elapsed   " +
                "Laps  " +
                "Corrected  " +
                "Place  " +
                "Achd  " +
                "  PI   " +
                "New   " +
                "C " +
                "A " +
                "PY";
            tsize = e.Graphics.MeasureString(outs, sfnt);
            lmargin = (int)((ps.Width - tsize.Width) / 2 + ps.X);
            e.Graphics.DrawString(outs, sfnt, b, lmargin, resoff + tsize.Height);

            outs = "                    " +
                "                   " +
                "         " +
                "     " +
                "          " +
                "          " +
                "      " +
                "           " +
                "       " +
                "Hcap  " +
                "       " +
                "Hcap  " +
                "  " +
                "  " +
                " |";
            tsize = e.Graphics.MeasureString(outs, sfnt);
            e.Graphics.DrawString(outs.Replace("|", ""), sfnt, b, lmargin, resoff + tsize.Height * 2);

            e.Graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Black, 2), lmargin, (int)(resoff + tsize.Height * 3.5),
                lmargin + tsize.Width, (int)(resoff + tsize.Height * 3.5));

            for (int i = 0; i < race.Races.Items.Count; i++)
            {
                DataRow r = ((DataRowView)race.Races.Items[i]).Row;
                object o = r["corrected"];
                double el = (double)o;
                outs = r["boatname"].ToString().PadRight(19) + " " +
                    r["boatclass"].ToString().PadRight(18) + " " +
                    r["sailno"].ToString().PadRight(8) + " " +
                    r["open_handicap"].ToString().PadLeft(5) + " " +
                    r["fintime"].ToString().PadRight(8) + "  " +
                    HMS((int)r["elapsed"]) + "  " +
                    r["laps"].ToString().PadLeft(3) + "  " +
                    HMS((double)r["corrected"]).PadLeft(9) + "   " +
                    r["place"].ToString().PadLeft(4) + " " +
                    r["achieved_handicap"].ToString().PadLeft(6) + " " +
                    r["performance_index"].ToString().PadLeft(5) + "   " +
                    r["new_rolling_handicap"].ToString().PadLeft(4) + "  " +
                    r["c"].ToString().PadRight(1) + " " +
                    r["a"].ToString().PadRight(1) + " " +
                    r["handicap_status"].ToString().PadLeft(2);
                tsize = e.Graphics.MeasureString(outs, sfnt);
                e.Graphics.DrawString(outs, sfnt, b, lmargin, (float)(resoff + (tsize.Height * 1.5) * (i + 3.5)));
            }
            e.HasMorePages = false;
            PageNo += 1;
            while (PageNo < reds.Length)
            {
                race = reds[PageNo];
                if (race.Races.Items.Count > 0)
                {
                    e.HasMorePages = true;
                    break;
                }
                PageNo += 1;
            }
        }

        private string HMS(double t)
        {
            int s = (int)t % 60;
            int m = (int)t / 60;
            int h = m / 60;
            m = m % 60;
            return h.ToString().PadLeft(2, '0') + ':' +
                m.ToString().PadLeft(2, '0') + ':' +
                s.ToString().PadLeft(2, '0');
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ((DockPanel)Parent).Children.Remove(this);
        }

        private void ChooseBoats_Click(object sender, RoutedEventArgs e)
        {
            SelectBoats dlg = new SelectBoats(reds);
            bool? ret = dlg.ShowDialog();
            if (ret.Value)
            {
                for (int i = 0; i < reds.Length; i++)
                    reds[i].LoadGrid();
            }
        }
    }
}
