using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Services;

namespace OodHelper.ViewModels
{
    public partial class SeriesViewModel : ObservableObject
    {
        private readonly ISeriesRepository _repository;
        private readonly IDialogService _dialogs;
        private CancellationTokenSource _debounce;

        internal int DebounceMilliseconds { get; set; } = 500;

        internal Task FilterTask { get; private set; }

        public SeriesViewModel(ISeriesRepository repository, IDialogService dialogs)
        {
            _repository = repository;
            _dialogs = dialogs;
        }

        [ObservableProperty]
        private string _filterText;

        [ObservableProperty]
        private ObservableCollection<Series> _rows;

        [ObservableProperty]
        private Series _selectedRow;

        partial void OnFilterTextChanged(string value)
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
            Rows = items == null ? null : new ObservableCollection<Series>(items);
        }

        [RelayCommand]
        private void Add()
        {
            if (_dialogs.ShowSeriesEditor(0))
                Load();
        }

        [RelayCommand]
        private void Edit()
        {
            if (SelectedRow == null) return;
            if (_dialogs.ShowSeriesEditor(SelectedRow.Sid))
                Load();
        }

        [RelayCommand]
        private void Delete(IList selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0) return;

            var change = false;
            foreach (var item in selectedItems.Cast<Series>().ToArray())
            {
                bool? result = _dialogs.ConfirmYesNoCancel(
                    "Are you sure you want to delete " + item.Sname + "?", "Confirm Delete");
                if (result == null) break;
                if (result == true)
                {
                    _repository.Delete(item.Sid);
                    change = true;
                }
            }

            if (change) Load();
        }
    }
}
