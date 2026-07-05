using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;

namespace OodHelper.ViewModels
{
    /// <summary>A calendar race row in the race-select grid, with a checkable membership flag.</summary>
    public partial class SeriesRaceCandidateViewModel : ObservableObject
    {
        public int Rid { get; }
        public string? Event { get; }
        public string? EventClass { get; }
        public DateTime? StartDate { get; }

        [ObservableProperty]
        private bool _selected;

        public SeriesRaceCandidateViewModel(SeriesRaceCandidate race)
        {
            Rid = race.Rid;
            Event = race.Event;
            EventClass = race.EventClass;
            StartDate = race.StartDate;
            Selected = race.Selected;
        }
    }

    /// <summary>
    /// Picks which calendar races belong to a series. Replaces the old DataView/RowFilter
    /// code-behind with a filtered <see cref="ICollectionView"/>.
    /// </summary>
    public partial class SeriesRaceSelectViewModel : ObservableObject
    {
        private readonly ISeriesRepository _repository;
        private readonly int _sid;

        /// <summary>Raised when the dialog should close; argument is the DialogResult.</summary>
        public event Action<bool>? CloseRequested;

        public ObservableCollection<SeriesRaceCandidateViewModel> Races { get; } =
            new ObservableCollection<SeriesRaceCandidateViewModel>();

        public ICollectionView RacesView { get; }

        [ObservableProperty]
        private string? _filterText;

        public SeriesRaceSelectViewModel(ISeriesRepository repository, int sid)
        {
            _repository = repository;
            _sid = sid;

            RacesView = CollectionViewSource.GetDefaultView(Races);
            RacesView.Filter = Matches;
        }

        public void Load()
        {
            Races.Clear();
            foreach (var race in _repository.GetAllRacesWithMembership(_sid))
                Races.Add(new SeriesRaceCandidateViewModel(race));
            RacesView.Refresh();
        }

        private bool Matches(object item)
        {
            if (string.IsNullOrWhiteSpace(FilterText)) return true;
            var race = (SeriesRaceCandidateViewModel)item;
            var f = FilterText.Trim();
            return (race.Event != null && race.Event.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0)
                   || (race.EventClass != null && race.EventClass.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        partial void OnFilterTextChanged(string? value)
        {
            RacesView.Refresh();
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (SeriesRaceCandidateViewModel race in RacesView)
                race.Selected = true;
        }

        [RelayCommand]
        private void Save()
        {
            _repository.SetMemberRaces(_sid, Races.Where(r => r.Selected).Select(r => r.Rid));
            CloseRequested?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }
    }
}
