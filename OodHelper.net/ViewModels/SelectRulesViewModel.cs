using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;
using OodHelper.Services;

namespace OodHelper.ViewModels
{
    public partial class SelectRulesViewModel : ObservableObject
    {
        private readonly ISelectRuleRepository _rules;
        private readonly IDialogService _dialogs;
        private CancellationTokenSource _debounce;

        internal int DebounceMilliseconds { get; set; } = 500;

        internal Task FilterTask { get; private set; }

        public SelectRulesViewModel(ISelectRuleRepository rules, IDialogService dialogs)
        {
            _rules = rules;
            _dialogs = dialogs;
        }

        [ObservableProperty]
        private string _filterText;

        [ObservableProperty]
        private ObservableCollection<SelectRuleListItem> _rows;

        [ObservableProperty]
        private SelectRuleListItem _selectedRow;

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
            var items = _rules.GetTopLevel(FilterText ?? string.Empty);
            Rows = items == null ? null : new ObservableCollection<SelectRuleListItem>(items);
        }

        [RelayCommand]
        private void Add()
        {
            if (_dialogs.ShowSelectRuleEditor(null))
                Load();
        }

        [RelayCommand]
        private void Edit()
        {
            if (SelectedRow == null) return;
            if (_dialogs.ShowSelectRuleEditor(SelectedRow.Id))
                Load();
        }

        [RelayCommand]
        private void Delete(IList selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0) return;

            var change = false;
            foreach (var item in selectedItems.Cast<SelectRuleListItem>().ToArray())
            {
                bool? result = _dialogs.ConfirmYesNoCancel(
                    "Are you sure you want to delete " + item.Name + "?", "Confirm Delete");
                if (result == null) break;
                if (result == true)
                {
                    _rules.Delete(item.Id);
                    change = true;
                }
            }

            if (change) Load();
        }
    }
}
