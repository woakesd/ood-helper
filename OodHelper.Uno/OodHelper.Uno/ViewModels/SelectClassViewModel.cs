using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;
using OodHelper.Data.Entities;

namespace OodHelper.ViewModels
{
    /// <summary>
    /// Picker for a Portsmouth-number class. Returns the chosen <see cref="SelectedId"/> to the
    /// caller (boat / race editors) via the <see cref="CloseRequested"/> dialog pattern.
    /// </summary>
    public partial class SelectClassViewModel : ObservableObject
    {
        private readonly IPortsmouthNumberRepository _repository;
        private CancellationTokenSource? _debounce;

        internal int DebounceMilliseconds { get; set; } = 500;

        internal Task? FilterTask { get; private set; }

        /// <summary>Raised when the dialog should close; argument is the DialogResult.</summary>
        public event Action<bool>? CloseRequested;

        public Guid? SelectedId => SelectedRow?.Id;

        public SelectClassViewModel(IPortsmouthNumberRepository repository)
        {
            _repository = repository;
        }

        [ObservableProperty]
        private string? _filterText;

        [ObservableProperty]
        private ObservableCollection<PortsmouthNumber>? _rows;

        [ObservableProperty]
        private PortsmouthNumber? _selectedRow;

        partial void OnFilterTextChanged(string? value)
        {
            FilterTask = DebouncedLoadAsync();
        }

        private async Task DebouncedLoadAsync()
        {
            _debounce?.Cancel();
            _debounce = new CancellationTokenSource();
            try
            {
                await Task.Delay(DebounceMilliseconds, _debounce.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            Load();
        }

        public void Load()
        {
            var items = _repository.GetAll(FilterText ?? string.Empty);
            Rows = items == null ? null : new ObservableCollection<PortsmouthNumber>(items);
        }

        [RelayCommand]
        private void Select()
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
