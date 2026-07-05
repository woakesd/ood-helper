using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;

namespace OodHelper.ViewModels
{
    /// <summary>
    /// Editor for a single race's notes (the calendar <c>memo</c>). Replaces the old RaceNotes
    /// code-behind: event/class are shown read-only and only the memo is editable and saved, via
    /// <see cref="ICalendarRepository"/> (no <c>Db</c>).
    /// </summary>
    public partial class RaceNotesViewModel : ObservableObject
    {
        private readonly ICalendarRepository _repository;
        private readonly int _rid;

        /// <summary>Raised when the dialog should close; argument is the DialogResult.</summary>
        public event Action<bool>? CloseRequested;

        public string? EventName { get; }
        public string? ClassName { get; }

        [ObservableProperty]
        private string? _memo;

        public RaceNotesViewModel(ICalendarRepository repository, int rid)
        {
            _repository = repository;
            _rid = rid;

            var calendar = repository.Get(rid);
            if (calendar != null)
            {
                EventName = calendar.Event;
                ClassName = calendar.Class;
                Memo = calendar.Memo;
            }
        }

        [RelayCommand]
        private void Save()
        {
            _repository.UpdateMemo(_rid, Memo);
            CloseRequested?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }
    }
}
