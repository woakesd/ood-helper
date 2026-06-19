using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;
using OodHelper.Data.Entities;

namespace OodHelper.ViewModels
{
    /// <summary>
    /// Picks a series and returns its sid (consumed by the Series-Results flow). Reuses
    /// <see cref="ISeriesRepository.GetAll"/>.
    /// </summary>
    public partial class SeriesChooserViewModel : ObservableObject
    {
        private readonly ISeriesRepository _repository;

        /// <summary>Raised when the dialog should close; argument is the DialogResult.</summary>
        public event Action<bool> CloseRequested;

        public int? SelectedSid => SelectedRow?.Sid;

        public SeriesChooserViewModel(ISeriesRepository repository)
        {
            _repository = repository;
        }

        [ObservableProperty]
        private ObservableCollection<Series> _rows;

        [ObservableProperty]
        private Series _selectedRow;

        public void Load()
        {
            var items = _repository.GetAll(string.Empty);
            Rows = items == null ? null : new ObservableCollection<Series>(items);
        }

        [RelayCommand]
        private void Choose()
        {
            CloseRequested?.Invoke(SelectedRow != null);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }
    }
}
