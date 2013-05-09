using System;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Printing;
using System.Windows.Xps;
using OodHelper.Maintain;
using OodHelper.Results.Model;
using OodHelper.Results.ViewModel;
using OodHelper.Helpers;

namespace OodHelper.Results
{
    /// <summary>
    /// Interaction logic for RaceResults.xaml
    /// </summary>
    public partial class RaceResults : UserControl
    {
        private ResultEditorViewModel[] reds;

        public RaceResults(int[] rids)
        {
            InitializeComponent();

            reds = new ViewModel.ResultEditorViewModel[rids.Length];

            //bool askAutoPopulate = true, doAutoPopulate = false;
            for (int i = 0; i < rids.Length; i++)
            {
                ICalendarEvent _event = new CalendarEvent(rids[i]);
                IList<IEntry> _entries = Entry.GetEntries(rids[i], null, _event.start_date);
                IRace _race = new Race(_event, _entries);
                ResultEditorViewModel _resultEditor = new ResultEditorViewModel(_race);

                reds[i] = _resultEditor;
                TabItem t = new TabItem();
                t.Header = _resultEditor.DisplayName;
                t.Content = _resultEditor;
                raceTabControl.Items.Add(t);
            }

            for (int i = 0; i < rids.Length; i++)
            {
                ResultEditorViewModel from = ((TabItem)raceTabControl.Items[i]).Content as ResultEditorViewModel;
                IList<CommandListItem> _commandList = from.ContextMenuItems;

                if (rids.Length > 1)
                {
                    for (int j = 0; j < rids.Length; j++)
                    {
                        ResultEditorViewModel to = ((TabItem)raceTabControl.Items[j]).Content as ResultEditorViewModel;
                        if (i != j)
                        {
                            CommandListItem _moveBoat = new CommandListItem();
                            _moveBoat.Text = string.Format("Move to {0}", to.DisplayName);
                            _moveBoat.Command = new RelayCommand(execute => from.MoveEntry(to), canExecute => from.CanMoveEntry());
                            _commandList.Add(_moveBoat);
                        }
                    }
                }
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            ResultsPrintSelector rps = new ResultsPrintSelector(reds);
            if (rps.ShowDialog() == true)
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    Working w = new Working(App.Current.MainWindow);
                    w.Show();
                    XpsDocumentWriter write = PrintQueue.CreateXpsDocumentWriter(pd.PrintQueue);
                    VisualsToXpsDocument collator = write.CreateVisualsCollator() as VisualsToXpsDocument;
                    Size ps = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                    collator.BeginBatchWrite();
                    System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            w.SetRange(0, reds.Length);
                            for (int i = 0; i < reds.Length; i++)
                            {
                                ViewModel.ResultEditorViewModel red = reds[i];
                                if (red.PrintInclude)
                                {
                                    string msg = null;
                                    Dispatcher.Invoke(new Action(delegate()
                                    {
                                        msg = string.Format("Printing {0}", red.DisplayName);
                                    }));
                                    w.SetProgress(msg, i + 1);
                                    System.Threading.Thread.Sleep(50);
                                    Dispatcher.Invoke(new Action(delegate()
                                    {
                                        Page p = null;
                                        IResultsPage rp = null;

                                        switch (red.Handicapping)
                                        {
                                            case Model.CalendarEvent.Handicappings.o:
                                                //p = (Page)new OpenHandicapResultsPage(red);
                                                break;
                                            case Model.CalendarEvent.Handicappings.r:
                                                //p = (Page)new RollingHandicapResultsPage(red);
                                                break;
                                        }
                                        rp = p as IResultsPage;
                                        p.Width = pd.PrintableAreaWidth;
                                        p.Measure(ps);
                                        p.Arrange(new Rect(new Point(0, 0), ps));
                                        p.UpdateLayout();

                                        int pno = 1;
                                        while (rp.PrintPage(collator, pno)) pno++;
                                    }));
                                }
                            }
                            Dispatcher.Invoke(new Action(delegate()
                            {
                                collator.EndBatchWrite();
                            }));
                            w.CloseWindow();
                        });
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DockPanel d = Parent as DockPanel;
            if (d != null)
                d.Children.Remove(this);
            else
            {
                TabItem t = Parent as TabItem;
                TabControl tc = t.Parent as TabControl;
                if (tc != null)
                    tc.Items.Remove(t);
                t.Content = null;
            }
        }

        private void ChooseBoats_Click(object sender, RoutedEventArgs e)
        {
            //SelectBoats dlg = new SelectBoats(reds);
            //if (dlg.ShowDialog() == true)
            //{
            //    for (int i = 0; i < reds.Length; i++)
            //        reds[i].LoadGrid();
            //}
        }
    }
}
