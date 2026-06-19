using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
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
            var chooser = new SeriesChooser();
            if (chooser.ShowDialog() != true) return null;
            return chooser.Sid;
        }

        public bool ShowSelectRuleEditor(Guid? id)
        {
            var vm = _services.GetRequiredService<Func<Guid?, SelectRuleEditViewModel>>()(id);
            var view = new SelectRuleEdit(vm) { Owner = Application.Current.MainWindow };
            return view.ShowDialog() == true;
        }
    }
}
