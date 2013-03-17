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
    public partial class PeopleList : Window, INotifyPropertyChanged
    {
        public PeopleList()
        {
            InitializeComponent();
            SelectMode = false;
            DataContext = this;
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

        public PeopleList(int? id)
            : this()
        {
            Id = id;
        }
        public PeopleList(bool selectMode, int? id)
            : this(id)
        {
            SelectMode = selectMode;
        }

        private void LoadGrid()
        {
            FilterPeople();
        }

        private void SetGridSource(WebService.People[] Persons)
        {
            PeopleData.ItemsSource = Persons;
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

        private void NextPage(object sender, RoutedEventArgs e)
        {
            Page++;
            FilterPeople();
        }

        public bool PreviousPageEnabled
        {
            get
            {
                return _page > 1;
            }
        }

        private void PreviousPage(object sender, RoutedEventArgs e)
        {
            if (Page > 1)
                Page--;
            FilterPeople();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (PeopleData.SelectedItem != null)
            {
                WebService.People i = PeopleData.SelectedItem as WebService.People;
                if (i.id == i.sid)
                {
                    PersonView p = new PersonView(i.id);
                    if (p.ShowDialog().Value)
                    {
                        LoadGrid();
                    }
                }
                else
                {
                    FamilyMember f = new FamilyMember(i.id, i.sid.Value);
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
                        WebService.People.DeletePeople((int)i.Row["id"]);
                        //Db del = new Db("DELETE FROM people " +
                        //    "WHERE id = @id");
                        //Hashtable d = new Hashtable();
                        //d["id"] = (int)i.Row["id"];
                        //del.ExecuteNonQuery(d);
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
                Dispatcher.Invoke(new dFilterPeople(FilterPeople), null);
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        public delegate void dFilterPeople();

        private int _page = 1;
        public int Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
                OnPropertyChanged("PreviousPageEnabled");
                OnPropertyChanged("NextPageEnabled");
            }
        }

        public void FilterPeople()
        {
            try
            {
                WebService.People[] _ppl;
                if (Peoplename.Text != string.Empty)
                {
                    _ppl = WebService.People.GetPeople(Peoplename.Text, Page);
                }
                else
                {
                    _ppl = WebService.People.GetPeople(Page);
                }
                SetGridSource(_ppl);
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
                WebService.People i = PeopleData.SelectedItem as WebService.People;
                if (i.id == i.sid)
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
