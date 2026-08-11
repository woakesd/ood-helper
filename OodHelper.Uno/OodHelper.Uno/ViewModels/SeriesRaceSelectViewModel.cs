using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Picks which calendar races belong to a series. The WPF version used an
    /// <c>ICollectionView</c>/<c>CollectionViewSource</c> to filter in place; under WinUI/Uno there is
    /// no <c>ICollectionView</c> in the shared VM, so <see cref="Races"/> holds every candidate and
    /// <see cref="FilteredRaces"/> (the collection the grid binds to) is rebuilt as the filter changes.
    /// </summary>
    public partial class SeriesRaceSelectViewModel : ObservableObject
    {
        private readonly ISeriesRepository _repository;
        private readonly int _sid;

        /// <summary>Raised when the dialog should close; argument is the DialogResult.</summary>
        public event Action<bool>? CloseRequested;

        /// <summary>Every calendar race (the unfiltered master list); selection state lives here.</summary>
        public ObservableCollection<SeriesRaceCandidateViewModel> Races { get; } =
            new ObservableCollection<SeriesRaceCandidateViewModel>();

        /// <summary>The races currently visible after applying <see cref="FilterText"/>; bound by the grid.</summary>
        public ObservableCollection<SeriesRaceCandidateViewModel> FilteredRaces { get; } =
            new ObservableCollection<SeriesRaceCandidateViewModel>();

        [ObservableProperty]
        private string? _filterText;

        public SeriesRaceSelectViewModel(ISeriesRepository repository, int sid)
        {
            _repository = repository;
            _sid = sid;
        }

        public void Load()
        {
            Races.Clear();
            foreach (var race in _repository.GetAllRacesWithMembership(_sid))
                Races.Add(new SeriesRaceCandidateViewModel(race));
            ApplyFilter();
        }

        private bool Matches(SeriesRaceCandidateViewModel race)
        {
            if (string.IsNullOrWhiteSpace(FilterText)) return true;
            var f = FilterText.Trim();
            return (race.Event != null && race.Event.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0)
                   || (race.EventClass != null && race.EventClass.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void ApplyFilter()
        {
            FilteredRaces.Clear();
            foreach (var race in Races.Where(Matches))
                FilteredRaces.Add(race);
        }

        partial void OnFilterTextChanged(string? value)
        {
            ApplyFilter();
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (SeriesRaceCandidateViewModel race in FilteredRaces)
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
