using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
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
    /// Interaction logic for SelectedBoats.xaml
    /// </summary>
    [Svn("$Id$")]
    public partial class SelectedBoats : UserControl
    {
        int raceId;

        public int RaceId
        {
            get { return raceId; }
        }

        public SelectedBoats(int rid)
        {
            InitializeComponent();
            raceId = rid;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            RemoveBoats();
        }

        private void Boats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemoveBoats();
        }

        public void RemoveBoats()
        {
            IList x = Boats.SelectedItems;
            DataRow[] rows = new DataRow[x.Count];
            int i = 0;
            foreach (DataRowView rv in x)
            {
                rows[i++] = rv.Row;
            }
            foreach (DataRow r in rows)
            {
                ((DataView)Boats.ItemsSource).Table.Rows.Remove(r);
            }
        }
    }
}
