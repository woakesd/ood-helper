using System;
using System.Threading.Tasks;

namespace OodHelper.Services
{
    /// <summary>
    /// UI-agnostic dialog surface used by the shared view-models. This is a trimmed, WinUI-shaped
    /// port of the WPF <c>IDialogService</c>: WinUI dialogs are inherently asynchronous
    /// (<c>ContentDialog.ShowAsync</c>), so every method is async rather than the WPF blocking
    /// <c>ShowDialog</c>. Only the members the walking-skeleton screens use are defined here; the
    /// rest of the WPF surface (progress dialogs, file pickers, the remaining editors) is deferred.
    /// </summary>
    public interface IDialogService
    {
        Task ShowErrorAsync(string message, string caption);
        Task ShowInformationAsync(string message, string caption);

        /// <summary>Yes = true, No = false, Cancel = null.</summary>
        Task<bool?> ConfirmYesNoCancelAsync(string message, string caption);

        /// <summary>
        /// Opens the series management list as a modal overlay, mirroring the WPF
        /// <c>ShowDialog&lt;Series&gt;</c> behaviour. Completes when the screen is closed.
        /// </summary>
        Task ShowSeriesAsync();

        /// <summary>
        /// Opens the single-series editor as a modal overlay (new series when sid is 0). Returns true
        /// if the series was saved (so a caller list can refresh).
        /// </summary>
        Task<bool> ShowSeriesEditorAsync(int sid);

        /// <summary>
        /// Opens the handicaps (class list) screen as a modal overlay, mirroring the WPF
        /// <c>ShowDialog&lt;Handicaps&gt;</c> behaviour. Completes when the screen is closed.
        /// </summary>
        Task ShowHandicapsAsync();

        /// <summary>Opens the series race-membership picker. Returns true if saved.</summary>
        Task<bool> ShowSeriesRaceSelectAsync(int sid);

        /// <summary>Opens the class picker. Returns the chosen class id, or null if cancelled.</summary>
        Task<Guid?> ShowClassPickerAsync();

        /// <summary>
        /// Opens the handicap (class) editor (new class when id is null). Not yet ported in the
        /// walking skeleton — the implementation surfaces a "not yet available" notice and returns false.
        /// </summary>
        Task<bool> ShowHandicapEditorAsync(Guid? id);
    }
}
