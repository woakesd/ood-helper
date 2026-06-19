using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Services;

namespace OodHelper.ViewModels
{
    /// <summary>
    /// Editor for a single series: name + discard profile, and the read-only list of member races
    /// (edited via the separate race-select dialog). Replaces the old SeriesEdit code-behind.
    /// </summary>
    public partial class SeriesEditViewModel : ObservableObject
    {
        private readonly ISeriesRepository _repository;
        private readonly IDialogService _dialogs;

        /// <summary>Raised when the dialog should close; argument is the DialogResult.</summary>
        public event Action<bool> CloseRequested;

        public int Sid { get; private set; }

        [ObservableProperty]
        private string _sname;

        [ObservableProperty]
        private string _discards;

        public ObservableCollection<SeriesRaceItem> Calendar { get; } = new ObservableCollection<SeriesRaceItem>();

        public SeriesEditViewModel(ISeriesRepository repository, IDialogService dialogs, int sid)
        {
            _repository = repository;
            _dialogs = dialogs;
            Sid = sid;

            if (Sid != 0)
            {
                var series = repository.Get(Sid);
                if (series != null)
                {
                    Sname = series.Sname;
                    Discards = series.Discards;
                }
                ReloadCalendar();
            }
        }

        private void ReloadCalendar()
        {
            Calendar.Clear();
            foreach (var race in _repository.GetMemberRaces(Sid))
                Calendar.Add(race);
        }

        [RelayCommand]
        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Sname))
            {
                _dialogs.ShowError("Series name cannot be blank", "Validation error");
                return;
            }

            Sid = _repository.Save(new Series { Sid = Sid, Sname = Sname, Discards = Discards });
            CloseRequested?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }

        [RelayCommand]
        private void EditRaces()
        {
            //
            // Race membership is keyed on a real sid, so the series must be saved first. The old
            // code silently wrote join rows against sid 0 for a brand-new series; guard instead.
            //
            if (Sid == 0)
            {
                _dialogs.ShowError("Save the series before editing its races", "Cannot edit races");
                return;
            }

            if (_dialogs.ShowSeriesRaceSelect(Sid))
                ReloadCalendar();
        }
    }
}
