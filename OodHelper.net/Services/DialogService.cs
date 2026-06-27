using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using OodHelper.Data;
using OodHelper.Maintain;
using OodHelper.Rules;
using OodHelper.ViewModels;

namespace OodHelper.Services
{
    internal sealed class DialogService : IDialogService
    {
        private readonly IServiceProvider _services;

        public DialogService(IServiceProvider services)
        {
            _services = services;
        }

        public bool Confirm(string message, string caption)
        {
            return MessageBox.Show(message, caption, MessageBoxButton.OKCancel,
                MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK;
        }

        public bool? ConfirmYesNoCancel(string message, string caption)
        {
            switch (MessageBox.Show(message, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes: return true;
                case MessageBoxResult.No: return false;
                default: return null;
            }
        }

        public void ShowError(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowInformation(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task<bool> ShowProgressAsync(string title,
            Func<IProgress<DownloadProgress>, CancellationToken, Task> work)
        {
            var owner = Application.Current.MainWindow;
            using var cts = new CancellationTokenSource();
            var dialog = new Working(owner, cts) { Title = title };
            //
            // Show the dialog non-modally (so we can keep awaiting) but disable the main window to give
            // it modal feel; the Progress<T> below captures the UI SynchronizationContext, so its
            // callback marshals progress updates back onto the dialog. The work itself runs on the
            // thread pool via Task.Run, keeping the UI responsive and the Cancel button live.
            //
            if (owner != null) owner.IsEnabled = false;
            dialog.Show();
            try
            {
                var progress = new Progress<DownloadProgress>(p => dialog.SetProgress(p.Message, p.Percent));
                await Task.Run(() => work(progress, cts.Token), cts.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            finally
            {
                if (owner != null) owner.IsEnabled = true;
                dialog.Close();
            }
        }

        public bool? ShowDialog<TWindow>() where TWindow : Window
        {
            var window = _services.GetRequiredService<TWindow>();
            if (window.Owner == null && !ReferenceEquals(window, Application.Current.MainWindow))
                window.Owner = Application.Current.MainWindow;
            return window.ShowDialog();
        }

        public BoatEditResult ShowBoatEditor(int bid)
        {
            var view = new BoatView(bid);
            var accepted = view.ShowDialog() == true;
            return new BoatEditResult
            {
                Accepted = accepted,
                BoatName = accepted ? ((BoatModel)view.DataContext).BoatName : null
            };
        }

        public int[] ShowRaceChooser()
        {
            var chooser = new RaceChooser();
            if (chooser.ShowDialog() != true) return null;
            return chooser.Rids;
        }

        public int? ShowSeriesChooser()
        {
            var vm = _services.GetRequiredService<SeriesChooserViewModel>();
            var view = new SeriesChooser(vm) { Owner = Application.Current.MainWindow };
            return view.ShowDialog() == true ? vm.SelectedSid : null;
        }

        public bool ShowSeriesEditor(int sid)
        {
            var vm = _services.GetRequiredService<Func<int, SeriesEditViewModel>>()(sid);
            var view = new SeriesEdit(vm) { Owner = Application.Current.MainWindow };
            return view.ShowDialog() == true;
        }

        public bool ShowSeriesRaceSelect(int sid)
        {
            var vm = _services.GetRequiredService<Func<int, SeriesRaceSelectViewModel>>()(sid);
            var view = new SeriesRaceSelect(vm) { Owner = Application.Current.MainWindow };
            return view.ShowDialog() == true;
        }

        public bool ShowRaceEditor(int rid)
        {
            var vm = _services.GetRequiredService<Func<int, RaceEditViewModel>>()(rid);
            var view = new RaceEdit(vm) { Owner = Application.Current.MainWindow };
            return view.ShowDialog() == true;
        }

        public bool ShowRaceNotes(int rid)
        {
            var vm = _services.GetRequiredService<Func<int, RaceNotesViewModel>>()(rid);
            var view = new RaceNotes(vm) { Owner = Application.Current.MainWindow };
            return view.ShowDialog() == true;
        }

        public bool ShowSelectRuleEditor(Guid? id)
        {
            var vm = _services.GetRequiredService<Func<Guid?, SelectRuleEditViewModel>>()(id);
            var view = new SelectRuleEdit(vm) { Owner = Application.Current.MainWindow };
            return view.ShowDialog() == true;
        }

        public bool ShowHandicapEditor(Guid? id)
        {
            var vm = _services.GetRequiredService<Func<Guid?, HandicapEditViewModel>>()(id);
            var view = new Handicap(vm) { Owner = Application.Current.MainWindow };
            return view.ShowDialog() == true;
        }

        public Guid? ShowClassPicker()
        {
            var vm = _services.GetRequiredService<SelectClassViewModel>();
            var view = new SelectClass(vm) { Owner = Application.Current.MainWindow };
            return view.ShowDialog() == true ? vm.SelectedId : null;
        }

        public string PickOpenFile(string filter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = filter };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }
}
