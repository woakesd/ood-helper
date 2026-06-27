using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OodHelper.Data;
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
        private readonly IResultsDownloadService _download;
        private readonly IResultsUploadService _upload;
        private readonly IUpdateCheckService _updateCheck;

        public OodHelperWindowViewModel(IDialogService dialogs, INavigationService navigation,
            IDatabaseMaintenanceService dbMaintenance, IResultsDownloadService download,
            IResultsUploadService upload, IUpdateCheckService updateCheck)
        {
            _dialogs = dialogs;
            _navigation = navigation;
            _dbMaintenance = dbMaintenance;
            _download = download;
            _upload = upload;
            _updateCheck = updateCheck;
        }

        //
        // Called once when the main window loads: if the website's results are newer than the local
        // copy, offer to download them. Replaces the legacy CheckForUpdates BackgroundWorker; any failure
        // is logged and swallowed so a transient website problem never blocks startup.
        //
        public async Task CheckForUpdatesAsync()
        {
            try
            {
                var result = await _updateCheck.CheckAsync(System.Threading.CancellationToken.None);
                if (result.WebsiteIsNewer &&
                    _dialogs.Confirm("Website results are more up to date\nWould you like to download from Website",
                        "Confirm Download"))
                {
                    await RunDownloadAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex);
            }
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
        private async Task ShowSeriesResults()
        {
            var sid = _dialogs.ShowSeriesChooser();
            if (sid.HasValue)
                await _navigation.OpenSeriesResultsAsync(sid.Value);
        }

        [RelayCommand]
        private async Task Download()
        {
            if (!_dialogs.Confirm("Click OK to confirm downloading database from Website", "Confirm Download"))
                return;
            await RunDownloadAsync();
        }

        //
        // Shared by the Download command and the startup "website is newer" prompt: drives the EF
        // bulk download behind the progress dialog and reports the outcome.
        //
        public async Task RunDownloadAsync()
        {
            try
            {
                var completed = await _dialogs.ShowProgressAsync("Downloading from Website",
                    (progress, ct) => _download.DownloadAsync(progress, ct));
                _dialogs.ShowInformation(completed ? "Download Complete" : "Download Cancelled",
                    completed ? "Finished" : "Cancel");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex);
                _dialogs.ShowError("Download Failed", "Failed");
            }
        }

        [RelayCommand]
        private async Task Upload()
        {
            if (!_dialogs.Confirm("Click OK to confirm uploading database to Website", "Confirm Upload"))
                return;
            await RunUploadAsync();
        }

        //
        // Drives the EF read / MySQL bulk upload behind the progress dialog and reports the outcome,
        // mirroring RunDownloadAsync.
        //
        public async Task RunUploadAsync()
        {
            try
            {
                var completed = await _dialogs.ShowProgressAsync("Uploading to Website",
                    (progress, ct) => _upload.UploadAsync(progress, ct));
                _dialogs.ShowInformation(completed ? "Upload Complete" : "Upload Cancelled",
                    completed ? "Finished" : "Cancel");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex);
                _dialogs.ShowError("Upload Failed", "Failed");
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

        [RelayCommand]
        private void OpenCalendar()
        {
            _dialogs.ShowDialog<Races>();
        }

        //
        // The windows below have not been converted to MVVM/DI yet; they are
        // constructed directly, exactly as the old Click handlers did.
        //

        [RelayCommand]
        private void OpenSeries()
        {
            _dialogs.ShowDialog<Series>();
        }

        [RelayCommand]
        private void OpenHandicaps()
        {
            _dialogs.ShowDialog<Handicaps>();
        }

        [RelayCommand]
        private void OpenRules()
        {
            _dialogs.ShowDialog<SelectRules>();
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
            _dialogs.ShowDialog<PnImport>();
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
