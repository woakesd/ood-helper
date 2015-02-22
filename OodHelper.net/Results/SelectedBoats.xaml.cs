using System.Collections;
using System.Data;
using System.Windows.Input;

namespace OodHelper.Results
{
    /// <summary>
    ///     Interaction logic for SelectedBoats.xaml
    /// </summary>
    public partial class SelectedBoats
    {
        private readonly int _raceId;

        public SelectedBoats(int rid)
        {
            InitializeComponent();
            _raceId = rid;
        }

        public int RaceId
        {
            get { return _raceId; }
        }

        private void Boats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemoveBoats();
        }

        public void RemoveBoats()
        {
            var x = Boats.SelectedItems;
            var rows = new DataRow[x.Count];
            var i = 0;
            foreach (DataRowView rv in x)
            {
                rows[i++] = rv.Row;
            }
            foreach (var r in rows)
            {
                ((DataView) Boats.ItemsSource).Table.Rows.Remove(r);
            }
        }
    }
}