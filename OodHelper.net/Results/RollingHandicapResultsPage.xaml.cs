﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps;
using OodHelper.Converters;

namespace OodHelper.Results
{
    /// <summary>
    /// Interaction logic for TestPage.xaml
    /// </summary>
    public partial class RollingHandicapResultsPage : Page, IResultsPage
    {
        DataTable rd;

        public RollingHandicapResultsPage(ResultsEditor red)
        {
            InitializeComponent();

            red.Scorer.Calculate(red.Rid);
            RaceDate.Text = "Date of Race: " + red.StartDate.Value.ToShortDateString();
            EventDescription.Text = red.RaceName;
            OodName.Text = "OOD: " + red.Ood;
            Start.Text = "Start: " + red.StartTime.ToString("hh\\:mm");
            Sct.Text = "SCT: " + Common.HMS(red.Scorer.StandardCorrectedTime);
            PrintedDate.Text = string.Format("Printed on {0:dd MMM yyyy} at {0:HH:mm:ss}", DateTime.Now);

            Db c = new Db(@"SELECT 0 [order], b.boatname Boat, b.boatclass [Class], b.sailno [Sail No], r.rolling_handicap as Hcap,
                r.finish_code, r.finish_date, '' AS Finish, r.elapsed Elapsed, r.laps Laps, r.corrected Corrected, r.place Pos,
                CASE WHEN ISNULL(override_points) = 1 THEN points ELSE override_points END Pts,
                r.achieved_handicap Achp, r.new_rolling_handicap [nhcp],
                ROUND((r.achieved_handicap - r.open_handicap) * 100.0 / r.open_handicap, 1) [%], r.c C, r.a A,
                r.handicap_status PY
                FROM boats b INNER JOIN races r ON r.bid = b.bid
                WHERE r.rid = @rid
                AND (finish_date IS NOT NULL OR finish_code IS NOT NULL)
                ORDER BY place");
            Hashtable p = new Hashtable();
            p["rid"] = red.Rid;

            rd = c.GetData(p);
            int i = 1;
            foreach (DataRow r in rd.Rows)
            {
                r["order"] = i++;
                if ((int)r["Pos"] == 999)
                {
                    r["elapsed"] = DBNull.Value;
                    r["laps"] = DBNull.Value;
                    r["corrected"] = DBNull.Value;
                    r["Pos"] = DBNull.Value;
                    r["Pts"] = DBNull.Value;
                    r["achp"] = DBNull.Value;
                    r["%"] = DBNull.Value;
                    r["nhcp"] = DBNull.Value;
                    r["c"] = DBNull.Value;
                    r["a"] = DBNull.Value;
                    r["py"] = DBNull.Value;
                }
                if (r["finish_code"] != DBNull.Value && r["finish_code"].ToString() != string.Empty)
                {
                    r["Finish"] = r["finish_code"];
                }
                else
                {
                    DateTime? fd = r["finish_date"] as DateTime?;
                    if (fd.HasValue)
                        r["Finish"] = fd.Value.ToString("HH:mm:ss");
                }
            }
            Results.ItemsSource = rd.DefaultView;
        }

        private int RowsPerPage;
        private ScrollBar vbar = null;

        public bool PrintPage(VisualsToXpsDocument collator, int pageNo)
        {
            if (pageNo == 1)
            {
                List<ScrollBar> sbs = Common.FindVisualChild<ScrollBar>(Results);
                vbar = sbs[0];
            }
            if (pageNo == 1 && vbar.ViewportSize >= vbar.Maximum)
            {
                collator.Write(this);
                return false;
            }
            else
            {
                try
                {
                    if (pageNo == 1)
                        RowsPerPage = (int)vbar.ViewportSize;
                    else
                        PageNumber.Text = string.Format("Page {0}", pageNo);
                    rd.DefaultView.RowFilter = string.Format("order >= {0} and order <= {1}", new object[] { (1 + (pageNo - 1) * RowsPerPage), (48 + (pageNo - 1) * RowsPerPage) });
                    UpdateLayout();
                    collator.Write(this);
                    if (pageNo * RowsPerPage >= rd.Rows.Count)
                        return false;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            return true;
        }

        private void Results_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGridColumn col1 = Results.Columns[rd.Columns["order"].Ordinal];
            col1.Visibility = System.Windows.Visibility.Collapsed;

            DataGridTextColumn col = (DataGridTextColumn)Results.Columns[rd.Columns["Elapsed"].Ordinal];
            Binding b = (Binding)col.Binding;
            b.StringFormat = "hh':'mm':'ss'";
            col.Binding = b;

            col = (DataGridTextColumn)Results.Columns[rd.Columns["Corrected"].Ordinal];
            b = (Binding)col.Binding;
            b.StringFormat = "hh':'mm':'ss'.'ff";
            col.Binding = b;

            col = (DataGridTextColumn)Results.Columns[rd.Columns["%"].Ordinal];
            b = (Binding)col.Binding;
            b.StringFormat = "0.#";
            col.Binding = b;

            Results.Columns[rd.Columns["finish_date"].Ordinal].Visibility = Visibility.Collapsed;
            Results.Columns[rd.Columns["finish_code"].Ordinal].Visibility = Visibility.Collapsed;

            SetRightAlignment(Results.Columns[rd.Columns["Laps"].Ordinal]);
            SetRightAlignment(Results.Columns[rd.Columns["hcap"].Ordinal]);
            SetRightAlignment(Results.Columns[rd.Columns["Pos"].Ordinal]);
            SetRightAlignment(Results.Columns[rd.Columns["Pts"].Ordinal]);
            SetRightAlignment(Results.Columns[rd.Columns["achp"].Ordinal]);
            SetRightAlignment(Results.Columns[rd.Columns["%"].Ordinal]);
            SetRightAlignment(Results.Columns[rd.Columns["nhcp"].Ordinal]);

            Hashtable widths = new Hashtable();
            widths["Boat"] = 22.0;
            widths["Class"] = 18.0;
            widths["Sail No"] = 10.0;
            widths["Hcap"] = 6.0;
            widths["Finish"] = 9.5;
            widths["Elapsed"] = 9.5;
            widths["Laps"] = 5.5;
            widths["Corrected"] = 10.5;
            widths["Pos"] = 5.0;
            widths["Pts"] = 5.0;
            widths["achp"] = 6.0;
            widths["%"] = 6.0;
            widths["nhcp"] = 6.0;
            widths["c"] = .75;
            widths["a"] = .75;
            widths["py"] = 1.5;

            double sumWidths = 0;
            foreach (double w in widths.Values)
                sumWidths += w;

            double printableWidth = Width - PageMainGrid.Margin.Left - PageMainGrid.Margin.Right;

            foreach (string colname in widths.Keys)
                Results.Columns[rd.Columns[colname].Ordinal].Width = printableWidth * ((double)widths[colname]) / sumWidths - 4;

            Results.Columns[rd.Columns["%"].Ordinal].Header = "%";
            Results.Columns[rd.Columns["achp"].Ordinal].Header = "Achd\nHcap";
            Results.Columns[rd.Columns["nhcp"].Ordinal].Header = "New\nHcap";
        }

        private void SetRightAlignment(DataGridColumn c)
        {
            c.CellStyle = this.Resources["RightAlignCell"] as Style;
            c.HeaderStyle = this.Resources["RightAlignHeader"] as Style;
        }

        private void Results_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Results.SelectedItems.Clear();
        }
    }
}
