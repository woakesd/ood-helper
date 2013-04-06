using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Threading.Tasks;

namespace OodHelper.Maintain
{
    /// <summary>
    /// Interaction logic for Boats.xaml
    /// </summary>
    public partial class People : Window, INotifyPropertyChanged
    {
        public People()
        {
            InitializeComponent();
            dSetGridSource = SetGridSource;
            SelectMode = false;
            DataContext = this;
            Width = System.Windows.SystemParameters.VirtualScreenWidth * 0.8;
            Height = System.Windows.SystemParameters.VirtualScreenHeight * 0.8;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        public int? Id { get; private set; }

        private bool? _selectMode;
        public bool? SelectMode
        {
            get
            {
                return _selectMode;
            }
            private set
            {
                _selectMode = value;
                OnPropertyChanged("SelectMode");
            }
        }

        public People(int? id)
            : this()
        {
            Id = id;
        }

        public People(bool selectMode, int? id)
            : this(id)
        {
            SelectMode = selectMode;
        }

        private delegate void DSetGridSource(DataTable ppl);
        private DSetGridSource dSetGridSource;
        Working w;

        private void LoadGrid()
        {
            PeopleData.ItemsSource = null;
            if (Peoplename.Text != string.Empty)
            {
                //w = new Working();
                //w.Show();
                string Filter = Peoplename.Text;
                //Task.Factory.StartNew(() =>
                //{
                    Db c = new Db(@"SELECT [id]
, [main_id]
, [firstname]
, [surname]
, [address1]
, [address2]
, [address3]
, [address4]
, [postcode]
, [hometel]
, [worktel]
, [mobile]
, [email]
, [club]
, [member]
, [manmemo]
, [cp]
, [s]
, [novice]
, [uid]
, [papernewsletter]
, [handbookexclude]
FROM people
WHERE firstname LIKE @filter
OR surname LIKE @filter
OR address1 LIKE @filter
OR address2 LIKE @filter
OR address3 LIKE @filter
OR address4 LIKE @filter
OR postcode LIKE @filter
OR hometel LIKE @filter
OR worktel LIKE @filter
OR mobile LIKE @filter
OR email LIKE @filter
OR club LIKE @filter
OR member LIKE @filter
ORDER BY surname, firstname");
                    Hashtable _para = new Hashtable();
                    _para["filter"] = string.Format("%{0}%", Filter);
                    DataTable ppl = c.GetData(_para);
                    c.Dispose();
                    SetGridSource(ppl);
                    //Dispatcher.Invoke(dSetGridSource, ppl);
                //});
            }
        }

        private void SetGridSource(DataTable ppl)
        {
            PeopleData.ItemsSource = ppl.DefaultView;
            if (Id != null)
            {
                foreach (DataRowView vr in PeopleData.Items)
                {
                    DataRow r = vr.Row;
                    if (((int)r["id"]) == Id.Value)
                    {
                        PeopleData.SelectedItem = vr;
                        PeopleData.ScrollIntoView(vr);
                        break;
                    }
                }
            }
            w.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            PersonView p = new PersonView(0);
            if (p.ShowDialog().Value)
            {
                LoadGrid();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
            {
                DataRowView i = (DataRowView) PeopleData.SelectedItem;
                if ((int)i.Row["id"] == (int)i.Row["main_id"])
                {
                    PersonView p = new PersonView((int)i.Row["id"]);
                    if (p.ShowDialog().Value)
                    {
                        LoadGrid();
                    }
                }
                else
                {
                    FamilyMember f = new FamilyMember((int)i.Row["id"], (int)i.Row["main_id"]);
                    if (f.ShowDialog().Value)
                    {
                        LoadGrid();
                    }
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
            {
                bool change = false;
                foreach (DataRowView i in PeopleData.SelectedItems)
                {
                    string name = i.Row["firstname"].ToString() + " " +
                        i.Row["surname"].ToString();
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + name + "?",
                        "Confirm Delete", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel) break;
                    if (result == MessageBoxResult.Yes)
                    {
                        Db del = new Db("DELETE FROM people " +
                            "WHERE id = @id");
                        Hashtable d = new Hashtable();
                        d["id"] = (int)i.Row["id"];
                        del.ExecuteNonQuery(d);
                        change = true;
                    }
                }
                if (change) LoadGrid();
            }
        }

        System.Timers.Timer t = null;

        void Peoplename_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (t == null)
            {
                t = new System.Timers.Timer(500);
                t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            }
            else
                t.Stop();
            t.AutoReset = false;
            t.Start();
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new dFilterPeople(FilterPeople), null);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        public delegate void dFilterPeople();

        public void FilterPeople()
        {
            try
            {
                LoadGrid();
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
            {
                DataRowView i = (DataRowView)PeopleData.SelectedItem;
                if ((int)i.Row["id"] == (int)i.Row["main_id"])
                    AddFamilyMember.IsEnabled = true;
                else
                    AddFamilyMember.IsEnabled = false;
            }
        }

        private void AddFamilyMember_Click(object sender, RoutedEventArgs e)
        {
            DataRowView i = (DataRowView) PeopleData.SelectedItem;
            int id = (int)i.Row["id"];
            int main_id = (int)i.Row["main_id"];
            string member = (string)i.Row["member"];
            if (id == main_id && member == "Family")
            {
                FamilyMember f = new FamilyMember(0, (int)i.Row["main_id"]);
                if (f.ShowDialog().Value)
                    LoadGrid();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGrid();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
                Peoplename.Focus();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView rv = PeopleData.SelectedItem as DataRowView;
            if (rv != null)
            {
                Id = rv.Row["id"] as int?;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("You must select a person first", "Please select", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void SetPaid_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to mark " + 
                    PeopleData.SelectedItems.Count.ToString() + " as paid up?",
                    "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Paid = true;
                    pd = new DataRowView[PeopleData.SelectedItems.Count];
                    PeopleData.SelectedItems.CopyTo(pd, 0);
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += new DoWorkEventHandler(bw_DoSetNotPaid);
                    SetNotPaid = new Working(bw);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_SetNotPaidCompleted);
                    bw.RunWorkerAsync();
                    SetNotPaid.ShowDialog();
                }
            }
        }

        Working SetNotPaid;
        bool Paid;
        DataRowView[] pd;

        private void SetNotPaid_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to mark " +
                    PeopleData.SelectedItems.Count.ToString() + " as not paid up?",
                    "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Paid = false;
                    pd = new DataRowView[PeopleData.SelectedItems.Count];
                    PeopleData.SelectedItems.CopyTo(pd, 0);
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += new DoWorkEventHandler(bw_DoSetNotPaid);
                    SetNotPaid = new Working(bw);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_SetNotPaidCompleted);
                    bw.RunWorkerAsync();
                    SetNotPaid.ShowDialog();
                }
            }
        }

        void bw_SetNotPaidCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetNotPaid.Close();
            LoadGrid();
        }

        void bw_DoSetNotPaid(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker w = sender as BackgroundWorker;
                int cnt = 0;
                foreach (DataRowView i in pd)
                {
                    Db upd = new Db("UPDATE people " +
                        "SET cp = @paid " +
                        "WHERE id = @id");
                    Hashtable d = new Hashtable();
                    d["id"] = (int)i.Row["id"];
                    d["paid"] = Paid;
                    upd.ExecuteNonQuery(d);
                    cnt++;
                    int progress = (int)Math.Round(cnt * 100.0 / pd.Length);
                    w.ReportProgress((int)Math.Round(cnt * 100.0 / pd.Length));

                    if (w.CancellationPending) return;
                }
                pd = null;
            }
            catch (Exception exp)
            {
                System.Windows.MessageBox.Show(exp.Message, "Error", System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
