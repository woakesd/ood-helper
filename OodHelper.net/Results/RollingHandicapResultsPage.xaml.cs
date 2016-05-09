﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Xps;
using OodHelper.Converters;

namespace OodHelper.Results
{
    /// <summary>
    ///     Interaction logic for TestPage.xaml
    /// </summary>
    public partial class RollingHandicapResultsPage : IResultsPage
    {
        private readonly DataTable _rd;

        private int _rowsPerPage;
        private ScrollBar _vbar;

        public RollingHandicapResultsPage(ResultsEditor red)
        {
            InitializeComponent();

            red.Scorer.Calculate(red.Rid);
            RaceDate.Text = "Date of Race: " + red.StartDate.ToShortDateString();
            EventDescription.Text = red.RaceName;
            OodName.Text = "OOD: " + red.Ood;
            Start.Text = "Start: " + red.StartTime.ToString("hh\\:mm");
            Sct.Text = "SCT: " + Common.HMS(red.Scorer.StandardCorrectedTime);
            PrintedDate.Text = string.Format("Printed on {0:dd MMM yyyy} at {0:HH:mm:ss}", DateTime.Now);

            var c =
                new Db(@"SELECT 0 [order], b.boatname + case when r.restricted_sail = 1 then ' (RS)' else '' end Boat,
                b.boatclass [Class], b.sailno [Sail No], r.rolling_handicap as Hcap,
                r.finish_code, r.finish_date, '' AS Finish, r.elapsed Elapsed, r.laps Laps, r.corrected Corrected, r.place Pos,
                ISNULL(override_points, points) Pts,
                r.achieved_handicap Achp, r.new_rolling_handicap [nhcp],
                ROUND((r.achieved_handicap - r.open_handicap) * 100.0 / r.open_handicap, 1) [%], r.c C, r.a A,
                r.handicap_status PY
                FROM boats b INNER JOIN races r ON r.bid = b.bid
                WHERE r.rid = @rid
                AND (finish_date IS NOT NULL OR finish_code IS NOT NULL)
                ORDER BY place");
            var p = new Hashtable();
            p["rid"] = red.Rid;

            _rd = c.GetData(p);
            int i = 1;
            foreach (DataRow r in _rd.Rows)
            {
                r["order"] = i++;
                if ((int) r["Pos"] == 999)
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
                    var fd = r["finish_date"] as DateTime?;
                    if (fd.HasValue)
                        r["Finish"] = fd.Value.ToString("HH:mm:ss");
                }
            }
            Results.ItemsSource = _rd.DefaultView;
        }

        public bool PrintPage(VisualsToXpsDocument collator, int pageNo)
        {
            if (pageNo == 1)
            {
                List<ScrollBar> sbs = Common.FindVisualChild<ScrollBar>(Results);
                _vbar = sbs[0];
            }
            if (pageNo == 1 && _vbar.ViewportSize >= _vbar.Maximum)
            {
                collator.Write(this);
                return false;
            }
            try
            {
                if (pageNo == 1)
                    _rowsPerPage = (int) _vbar.ViewportSize;
                else
                    PageNumber.Text = string.Format("Page {0}", pageNo);
                _rd.DefaultView.RowFilter = string.Format("order >= {0} and order <= {1}",
                    new object[] {(1 + (pageNo - 1)*_rowsPerPage), (48 + (pageNo - 1)*_rowsPerPage)});
                UpdateLayout();
                collator.Write(this);
                if (pageNo*_rowsPerPage >= _rd.Rows.Count)
                    return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return true;
        }

        private void Results_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGridColumn col1 = Results.Columns[_rd.Columns["order"].Ordinal];
            col1.Visibility = Visibility.Collapsed;

            var col = (DataGridTextColumn) Results.Columns[_rd.Columns["Elapsed"].Ordinal];
            var b = (Binding) col.Binding;
            b.Converter = new IntTimeSpan();
            col.Binding = b;

            col = (DataGridTextColumn) Results.Columns[_rd.Columns["Corrected"].Ordinal];
            b = (Binding) col.Binding;
            b.Converter = new DoubleTimeSpan();
            col.Binding = b;

            col = (DataGridTextColumn) Results.Columns[_rd.Columns["%"].Ordinal];
            b = (Binding) col.Binding;
            b.StringFormat = "0.#";
            col.Binding = b;

            Results.Columns[_rd.Columns["finish_date"].Ordinal].Visibility = Visibility.Collapsed;
            Results.Columns[_rd.Columns["finish_code"].Ordinal].Visibility = Visibility.Collapsed;

            SetRightAlignment(Results.Columns[_rd.Columns["Laps"].Ordinal]);
            SetRightAlignment(Results.Columns[_rd.Columns["hcap"].Ordinal]);
            SetRightAlignment(Results.Columns[_rd.Columns["Pos"].Ordinal]);
            SetRightAlignment(Results.Columns[_rd.Columns["Pts"].Ordinal]);
            SetRightAlignment(Results.Columns[_rd.Columns["achp"].Ordinal]);
            SetRightAlignment(Results.Columns[_rd.Columns["%"].Ordinal]);
            SetRightAlignment(Results.Columns[_rd.Columns["nhcp"].Ordinal]);

            var widths = new Hashtable();
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

            var sumWidths = widths.Values.Cast<double>().Sum();

            var printableWidth = Width - PageMainGrid.Margin.Left - PageMainGrid.Margin.Right;

            foreach (string colname in widths.Keys)
                Results.Columns[_rd.Columns[colname].Ordinal].Width = printableWidth*((double) widths[colname])/sumWidths -
                                                                     4;

            Results.Columns[_rd.Columns["%"].Ordinal].Header = "%";
            Results.Columns[_rd.Columns["achp"].Ordinal].Header = "Achd\nHcap";
            Results.Columns[_rd.Columns["nhcp"].Ordinal].Header = "New\nHcap";
        }

        private void SetRightAlignment(DataGridColumn c)
        {
            c.CellStyle = Resources["RightAlignCell"] as Style;
            c.HeaderStyle = Resources["RightAlignHeader"] as Style;
        }
    }
}