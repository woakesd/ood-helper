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
        /// <summary>Opens the series editor (new series when sid is 0). Returns true if saved.</summary>
        bool ShowSeriesEditor(int sid);
        /// <summary>Opens the series race-membership picker. Returns true if saved.</summary>
        bool ShowSeriesRaceSelect(int sid);
        /// <summary>Opens the rule editor (new rule when id is null). Returns true if saved.</summary>
        bool ShowSelectRuleEditor(Guid? id);
        /// <summary>Opens the handicap (class) editor (new class when id is null). Returns true if saved.</summary>
        bool ShowHandicapEditor(Guid? id);
        /// <summary>Opens the class picker. Returns the chosen class id, or null if cancelled.</summary>
        Guid? ShowClassPicker();
        /// <summary>Shows an open-file dialog with the given filter. Returns the path, or null if cancelled.</summary>
        string PickOpenFile(string filter);
    }
}
