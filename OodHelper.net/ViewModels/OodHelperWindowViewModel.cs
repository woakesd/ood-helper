using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.LoadTide;
using OodHelper.Maintain;
using OodHelper.Rules;
using OodHelper.Services;
using OodHelper.Sun;
using OodHelper.Website;

namespace OodHelper.ViewModels
{
    public partial class OodHelperWindowViewModel : ObservableObject
    {
        private readonly IDialogService _dialogs;
        private readonly INavigationService _navigation;
        private readonly IDatabaseMaintenanceService _dbMaintenance;

        public OodHelperWindowViewModel(IDialogService dialogs, INavigationService navigation,
            IDatabaseMaintenanceService dbMaintenance)
        {
            _dialogs = dialogs;
            _navigation = navigation;
            _dbMaintenance = dbMaintenance;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HideNonPrivilegedItems))]
        private bool _showPrivilegedItems;

        public bool HideNonPrivilegedItems => !ShowPrivilegedItems;

        [RelayCommand]
        private void Login()
        {
            ShowPrivilegedItems = true;
        }

        [RelayCommand]
        private void Logout()
        {
            ShowPrivilegedItems = false;
        }

        [RelayCommand]
        private void ShowRaceResults()
        {
            var rids = _dialogs.ShowRaceChooser();
            if (rids != null)
                _navigation.OpenRaceResults(rids);
        }

        [RelayCommand]
        private void ShowSeriesResults()
        {
            var sid = _dialogs.ShowSeriesChooser();
            if (sid.HasValue)
                _navigation.OpenSeriesResults(sid.Value);
        }

        [RelayCommand]
        private void Download()
        {
            if (_dialogs.Confirm("Click OK to confirm downloading database from Website", "Confirm Download"))
            {
                // ReSharper disable once ObjectCreationAsStatement
                new DownloadResults();
            }
        }

        [RelayCommand]
        private void Upload()
        {
            if (_dialogs.Confirm("Click OK to confirm uploading database to Website", "Confirm Upload"))
            {
                var worker = new UploadResults();
                worker.Run();
            }
        }

        [RelayCommand]
        private void RecreateDb()
        {
            _dbMaintenance.RecreateDatabase();
        }

        [RelayCommand]
        private void OpenBoats()
        {
            _dialogs.ShowDialog<Boats>();
        }

        [RelayCommand]
        private void OpenConfiguration()
        {
            _dialogs.ShowDialog<Configure>();
        }

        //
        // The windows below have not been converted to MVVM/DI yet; they are
        // constructed directly, exactly as the old Click handlers did.
        //

        [RelayCommand]
        private void OpenCalendar()
        {
            new Races().ShowDialog();
        }

        [RelayCommand]
        private void OpenSeries()
        {
            new Series().ShowDialog();
        }

        [RelayCommand]
        private void OpenHandicaps()
        {
            new Handicaps().ShowDialog();
        }

        [RelayCommand]
        private void OpenRules()
        {
            new SelectRules().ShowDialog();
        }

        [RelayCommand]
        private void OpenTide()
        {
            new ReadData().ShowDialog();
        }

        [RelayCommand]
        private void OpenSun()
        {
            new DoSunSetRise().ShowDialog();
        }

        [RelayCommand]
        private void OpenEntrySheets()
        {
            new EntrySheetSelector().ShowDialog();
        }

        [RelayCommand]
        private void ImportPy()
        {
            new PnImport().ShowDialog();
        }

        [RelayCommand]
        private void ExportResults()
        {
            // Not implemented; the old Click handler was empty too.
        }

        [RelayCommand]
        private void ShowAbout()
        {
            new About().ShowDialog();
        }
    }
}
