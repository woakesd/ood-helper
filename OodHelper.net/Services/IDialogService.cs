using System;
using System.Windows;

namespace OodHelper.Services
{
    public sealed class BoatEditResult
    {
        public bool Accepted { get; set; }
        public string BoatName { get; set; }
    }

    public interface IDialogService
    {
        bool Confirm(string message, string caption);
        /// <summary>Yes = true, No = false, Cancel = null.</summary>
        bool? ConfirmYesNoCancel(string message, string caption);
        void ShowError(string message, string caption);
        bool? ShowDialog<TWindow>() where TWindow : Window;
        BoatEditResult ShowBoatEditor(int bid);
        int[] ShowRaceChooser();
        int? ShowSeriesChooser();
        /// <summary>Opens the rule editor (new rule when id is null). Returns true if saved.</summary>
        bool ShowSelectRuleEditor(Guid? id);
    }
}
