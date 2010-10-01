using System;
using System.Collections;
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
using System.ComponentModel;

namespace OodHelper
{
    /// <summary>
    /// Interaction logic for EntrySheetSelector.xaml
    /// </summary>
    [Svn("$Id: EntrySheetSelector.xaml.cs 121 2010-08-15 23:19:35Z woakesdavid $")]
    public partial class ResultsPrintSelector : Window, INotifyPropertyChanged
    {
        private IPrintSelectItem[] Reds { get; set; }

        public ResultsPrintSelector(IPrintSelectItem[] reds)
        {
            InitializeComponent();
            Reds = reds;

            int Group = 0;

            if (reds.Length > 0)
            {
                Group = Reds[0].PrintIncludeGroup;
                Reds[0].PrintIncludeAllVisible = true;
                Reds[0].PrintIncludeAll = true;
            }
            for (int i = 0; i < Reds.Length; i++)
            {
                IPrintSelectItem p = null;
                if (i > 0) p = Reds[i - 1];
                IPrintSelectItem r = Reds[i];
                if (r.PrintIncludeGroup == Group)
                    r.PrintInclude = true;
                else
                    r.PrintInclude = false;
                if (i > 0 && r.PrintIncludeGroup != p.PrintIncludeGroup)
                    r.PrintIncludeAllVisible = true;
                else if (i > 0)
                    r.PrintIncludeAllVisible = false;
            }
            Races.ItemsSource = Reds;
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

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Include_All_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox;
            if (cb != null)
            {
                IPrintSelectItem r = cb.DataContext as IPrintSelectItem;
                foreach (IPrintSelectItem p in Reds)
                {
                    if (r.PrintIncludeGroup == r.PrintIncludeGroup)
                    {
                        p.PrintInclude = cb.IsChecked.Value;
                        p.OnPropertyChanged("PrintInclude");
                    }
                }
            }
        }

        private void Include_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox;
            if (cb != null)
            {
                IPrintSelectItem r = cb.DataContext as IPrintSelectItem;
                for (int i = 0; i < Reds.Length; i++)
                {
                    IPrintSelectItem p = Reds[i];
                    if (p.PrintIncludeGroup == r.PrintIncludeGroup
                        && p.PrintIncludeAllVisible
                        && cb.IsChecked == false)
                    {
                        p.PrintIncludeAll = false;
                        p.OnPropertyChanged("PrintIncludeAll");
                    }
                    if (p.PrintIncludeAllVisible)
                    {
                        bool allprint = true;
                        for (int j = i; j < Reds.Length; j++)
                        {
                            IPrintSelectItem q = Reds[j];
                            if (q.PrintIncludeGroup == p.PrintIncludeGroup &&
                                !q.PrintInclude)
                            {
                                allprint = false;
                                break;
                            }
                        }
                        p.PrintIncludeAll = allprint;
                        p.OnPropertyChanged("PrintIncludeAll");
                    }
                }
            }
        }
    }
}
