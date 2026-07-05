using System.Collections;
using System.Collections.Generic;
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
    public partial class BoatsViewModel : ObservableObject
    {
        private readonly IBoatRepository _boats;
        private readonly IDialogService _dialogs;
        private CancellationTokenSource _debounce;

        internal int DebounceMilliseconds { get; set; } = 500;

        internal Task FilterTask { get; private set; }

        public BoatsViewModel(IBoatRepository boats, IDialogService dialogs)
        {
            _boats = boats;
            _dialogs = dialogs;
        }

        [ObservableProperty]
        private string _filterText;

        [ObservableProperty]
        private ObservableCollection<Boat> _rows;

        [ObservableProperty]
        private Boat _selectedRow;

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
            if (FilterText != null && FilterText.Trim() != string.Empty)
                Rows = new ObservableCollection<Boat>(_boats.Search(FilterText));
            else
                Rows = null;
        }

        [RelayCommand]
        private void Add()
        {
            var result = _dialogs.ShowBoatEditor(0);
            if (result.Accepted)
            {
                FilterText = result.BoatName;
                Load();
            }
        }

        [RelayCommand]
        private void Edit()
        {
            if (SelectedRow == null) return;
            var result = _dialogs.ShowBoatEditor(SelectedRow.Bid);
            if (result.Accepted)
                Load();
        }

        [RelayCommand]
        private void Delete(IList selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0) return;
            var change = false;
            foreach (var boat in selectedItems.Cast<Boat>().ToArray())
            {
                bool? result = _dialogs.ConfirmYesNoCancel(
                    "Are you sure you want to delete " + boat.Boatname + "?", "Confirm Delete");
                if (result == null) break;
                if (result == true)
                {
                    _boats.Delete(boat.Bid);
                    change = true;
                }
            }
            if (change) Load();
        }
    }
}
