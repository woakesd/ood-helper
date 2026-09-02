using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Services;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class OodHelperWindowViewModelTests
    {
        private readonly IDialogService _dialogs = Substitute.For<IDialogService>();
        private readonly INavigationService _navigation = Substitute.For<INavigationService>();
        private readonly IDatabaseMaintenanceService _dbMaintenance = Substitute.For<IDatabaseMaintenanceService>();
        private readonly IResultsDownloadService _download = Substitute.For<IResultsDownloadService>();
        private readonly IResultsUploadService _upload = Substitute.For<IResultsUploadService>();
        private readonly IUpdateCheckService _updateCheck = Substitute.For<IUpdateCheckService>();
        private readonly IRaceExportService _raceExport = Substitute.For<IRaceExportService>();
        private readonly ISeriesResultExportService _seriesResultExport = Substitute.For<ISeriesResultExportService>();
        private readonly ISeriesRecalculationService _seriesRecalculation = Substitute.For<ISeriesRecalculationService>();

        private OodHelperWindowViewModel CreateViewModel()
        {
            return new OodHelperWindowViewModel(_dialogs, _navigation, _dbMaintenance, _download, _upload,
                _updateCheck, _raceExport, _seriesResultExport, _seriesRecalculation);
        }

        [Fact]
        public void LoginAndLogout_ToggleBothVisibilityProperties()
        {
            var vm = CreateViewModel();
            Assert.False(vm.ShowPrivilegedItems);
            Assert.True(vm.HideNonPrivilegedItems);

            vm.LoginCommand.Execute(null);
            Assert.True(vm.ShowPrivilegedItems);
            Assert.False(vm.HideNonPrivilegedItems);

            vm.LogoutCommand.Execute(null);
            Assert.False(vm.ShowPrivilegedItems);
            Assert.True(vm.HideNonPrivilegedItems);
        }

        [Fact]
        public void Login_RaisesPropertyChangedForHideNonPrivilegedItems()
        {
            var vm = CreateViewModel();
            var raised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(OodHelperWindowViewModel.HideNonPrivilegedItems))
                    raised = true;
            };

            vm.LoginCommand.Execute(null);

            Assert.True(raised);
        }

        [Fact]
        public void ShowRaceResults_OpensTab_WhenChooserReturnsRaces()
        {
            var rids = new[] { 1, 2, 3 };
            _dialogs.ShowRaceChooser().Returns(rids);
            var vm = CreateViewModel();

            vm.ShowRaceResultsCommand.Execute(null);

            _navigation.Received(1).OpenRaceResults(rids);
        }

        [Fact]
        public void ShowRaceResults_DoesNothing_WhenChooserCancelled()
        {
            _dialogs.ShowRaceChooser().Returns((int[])null);
            var vm = CreateViewModel();

            vm.ShowRaceResultsCommand.Execute(null);

            _navigation.DidNotReceive().OpenRaceResults(Arg.Any<int[]>());
        }

        [Fact]
        public async Task ShowSeriesResults_OpensTab_WhenChooserReturnsSeries()
        {
            _dialogs.ShowSeriesChooser().Returns(5);
            var vm = CreateViewModel();

            await vm.ShowSeriesResultsCommand.ExecuteAsync(null);

            await _navigation.Received(1).OpenSeriesResultsAsync(5);
        }

        [Fact]
        public async Task ShowSeriesResults_DoesNothing_WhenChooserCancelled()
        {
            _dialogs.ShowSeriesChooser().Returns((int?)null);
            var vm = CreateViewModel();

            await vm.ShowSeriesResultsCommand.ExecuteAsync(null);

            await _navigation.DidNotReceive().OpenSeriesResultsAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task Download_DoesNothing_WhenNotConfirmed()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
            var vm = CreateViewModel();

            await vm.DownloadCommand.ExecuteAsync(null);

            _dialogs.Received(1).Confirm(Arg.Any<string>(), "Confirm Download");
            _ = _dialogs.DidNotReceive().ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>());
        }

        [Fact]
        public async Task Download_WhenConfirmedAndCompletes_RunsProgressAndReportsComplete()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _dialogs.ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>()).Returns(Task.FromResult(true));
            var vm = CreateViewModel();

            await vm.DownloadCommand.ExecuteAsync(null);

            _ = _dialogs.Received(1).ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>());
            _dialogs.Received(1).ShowInformation("Download Complete", "Finished");
        }

        [Fact]
        public async Task Download_WhenCancelled_ReportsCancelled()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _dialogs.ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>()).Returns(Task.FromResult(false));
            var vm = CreateViewModel();

            await vm.DownloadCommand.ExecuteAsync(null);

            _dialogs.Received(1).ShowInformation("Download Cancelled", "Cancel");
        }

        [Fact]
        public async Task Upload_DoesNothing_WhenNotConfirmed()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
            var vm = CreateViewModel();

            await vm.UploadCommand.ExecuteAsync(null);

            _dialogs.Received(1).Confirm(Arg.Any<string>(), "Confirm Upload");
            _ = _dialogs.DidNotReceive().ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>());
        }

        [Fact]
        public async Task Upload_WhenConfirmedAndCompletes_RunsProgressAndReportsComplete()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _dialogs.ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>()).Returns(Task.FromResult(true));
            var vm = CreateViewModel();

            await vm.UploadCommand.ExecuteAsync(null);

            _ = _dialogs.Received(1).ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>());
            _dialogs.Received(1).ShowInformation("Upload Complete", "Finished");
        }

        [Fact]
        public async Task Upload_WhenCancelled_ReportsCancelled()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _dialogs.ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>()).Returns(Task.FromResult(false));
            var vm = CreateViewModel();

            await vm.UploadCommand.ExecuteAsync(null);

            _dialogs.Received(1).ShowInformation("Upload Cancelled", "Cancel");
        }

        [Fact]
        public async Task CheckForUpdates_DoesNothing_WhenWebsiteNotNewer()
        {
            // Equal dates: WebsiteIsNewer is false, so no prompt and no download.
            var now = DateTime.Now;
            _updateCheck.CheckAsync(Arg.Any<CancellationToken>())
                .Returns(new UpdateCheckResult(now, now));
            var vm = CreateViewModel();

            await vm.CheckForUpdatesAsync();

            _dialogs.DidNotReceive().Confirm(Arg.Any<string>(), Arg.Any<string>());
            _ = _dialogs.DidNotReceive().ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>());
        }

        [Fact]
        public async Task CheckForUpdates_DoesNotDownload_WhenWebsiteNewerButDeclined()
        {
            _updateCheck.CheckAsync(Arg.Any<CancellationToken>())
                .Returns(new UpdateCheckResult(DateTime.Now.AddDays(-1), DateTime.Now));
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
            var vm = CreateViewModel();

            await vm.CheckForUpdatesAsync();

            _dialogs.Received(1).Confirm(Arg.Any<string>(), "Confirm Download");
            _ = _dialogs.DidNotReceive().ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>());
        }

        [Fact]
        public async Task CheckForUpdates_RunsDownload_WhenWebsiteNewerAndConfirmed()
        {
            _updateCheck.CheckAsync(Arg.Any<CancellationToken>())
                .Returns(new UpdateCheckResult(DateTime.Now.AddDays(-1), DateTime.Now));
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _dialogs.ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>()).Returns(Task.FromResult(true));
            var vm = CreateViewModel();

            await vm.CheckForUpdatesAsync();

            _ = _dialogs.Received(1).ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>());
            _dialogs.Received(1).ShowInformation("Download Complete", "Finished");
        }

        [Fact]
        public void RecreateDb_DelegatesToMaintenanceService()
        {
            var vm = CreateViewModel();

            vm.RecreateDbCommand.Execute(null);

            _dbMaintenance.Received(1).RecreateDatabase();
        }

        [Fact]
        public async Task ExportRaceResults_DoesNothing_WhenSaveCancelled()
        {
            _dialogs.PickSaveFile(Arg.Any<string>(), Arg.Any<string>()).Returns((string)null);
            var vm = CreateViewModel();

            await vm.ExportRaceResultsCommand.ExecuteAsync(null);

            _ = _raceExport.DidNotReceive().ExportRacesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _dialogs.DidNotReceive().ShowInformation(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ExportRaceResults_ExportsToChosenPath_AndReportsRowCount()
        {
            _dialogs.PickSaveFile(Arg.Any<string>(), Arg.Any<string>()).Returns(@"C:\races.xlsx");
            _raceExport.ExportRacesAsync(@"C:\races.xlsx", Arg.Any<CancellationToken>()).Returns(Task.FromResult(42));
            var vm = CreateViewModel();

            await vm.ExportRaceResultsCommand.ExecuteAsync(null);

            _ = _raceExport.Received(1).ExportRacesAsync(@"C:\races.xlsx", Arg.Any<CancellationToken>());
            _dialogs.Received(1).ShowInformation(Arg.Is<string>(m => m.Contains("42")), "Export Complete");
        }

        [Fact]
        public async Task ExportRaceResults_ReportsError_WhenExportThrows()
        {
            _dialogs.PickSaveFile(Arg.Any<string>(), Arg.Any<string>()).Returns(@"C:\races.xlsx");
            _raceExport.ExportRacesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns<Task<int>>(_ => throw new IOException("locked"));
            var vm = CreateViewModel();

            await vm.ExportRaceResultsCommand.ExecuteAsync(null);

            _dialogs.Received(1).ShowError(Arg.Any<string>(), "Failed");
            _dialogs.DidNotReceive().ShowInformation(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ExportSeriesResults_DoesNothing_WhenSaveCancelled()
        {
            _dialogs.PickSaveFile(Arg.Any<string>(), Arg.Any<string>()).Returns((string)null);
            var vm = CreateViewModel();

            await vm.ExportSeriesResultsCommand.ExecuteAsync(null);

            _ = _seriesResultExport.DidNotReceive()
                .ExportSeriesResultsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            _dialogs.DidNotReceive().ShowInformation(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ExportSeriesResults_ExportsToChosenPath_AndReportsRowCount()
        {
            _dialogs.PickSaveFile(Arg.Any<string>(), Arg.Any<string>()).Returns(@"C:\series.xlsx");
            _seriesResultExport.ExportSeriesResultsAsync(@"C:\series.xlsx", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(17));
            var vm = CreateViewModel();

            await vm.ExportSeriesResultsCommand.ExecuteAsync(null);

            _ = _seriesResultExport.Received(1)
                .ExportSeriesResultsAsync(@"C:\series.xlsx", Arg.Any<CancellationToken>());
            _dialogs.Received(1).ShowInformation(Arg.Is<string>(m => m.Contains("17")), "Export Complete");
        }

        [Fact]
        public async Task ExportSeriesResults_ReportsError_WhenExportThrows()
        {
            _dialogs.PickSaveFile(Arg.Any<string>(), Arg.Any<string>()).Returns(@"C:\series.xlsx");
            _seriesResultExport.ExportSeriesResultsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns<Task<int>>(_ => throw new IOException("locked"));
            var vm = CreateViewModel();

            await vm.ExportSeriesResultsCommand.ExecuteAsync(null);

            _dialogs.Received(1).ShowError(Arg.Any<string>(), "Failed");
            _dialogs.DidNotReceive().ShowInformation(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task RecalculateSeriesResults_DoesNothing_WhenNotConfirmed()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
            var vm = CreateViewModel();

            await vm.RecalculateSeriesResultsCommand.ExecuteAsync(null);

            _dialogs.Received(1).Confirm(Arg.Any<string>(), "Confirm Recalculate");
            _ = _dialogs.DidNotReceive().ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>());
        }

        [Fact]
        public async Task RecalculateSeriesResults_WhenConfirmed_RunsRecalculationAndReportsCounts()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _seriesRecalculation.RecalculateAllAsync(Arg.Any<IProgress<DownloadProgress>>(),
                    Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new SeriesRecalculationSummary(7, 22, 0)));
            // Run the work delegate so the summary is captured, as the real dialog does.
            _dialogs.ShowProgressAsync(Arg.Any<string>(),
                    Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>())
                .Returns(async ci =>
                {
                    var work = ci.Arg<Func<IProgress<DownloadProgress>, CancellationToken, Task>>();
                    await work(new Progress<DownloadProgress>(), CancellationToken.None);
                    return true;
                });
            var vm = CreateViewModel();

            await vm.RecalculateSeriesResultsCommand.ExecuteAsync(null);

            _ = _seriesRecalculation.Received(1).RecalculateAllAsync(
                Arg.Any<IProgress<DownloadProgress>>(), Arg.Any<CancellationToken>());
            _dialogs.Received(1).ShowInformation(
                Arg.Is<string>(m => m.Contains("7") && m.Contains("22")), "Finished");
        }

        [Fact]
        public async Task RecalculateSeriesResults_ReportsFailures_WhenSomeSeriesFailed()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _seriesRecalculation.RecalculateAllAsync(Arg.Any<IProgress<DownloadProgress>>(),
                    Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new SeriesRecalculationSummary(5, 1, 2)));
            _dialogs.ShowProgressAsync(Arg.Any<string>(),
                    Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>())
                .Returns(async ci =>
                {
                    var work = ci.Arg<Func<IProgress<DownloadProgress>, CancellationToken, Task>>();
                    await work(new Progress<DownloadProgress>(), CancellationToken.None);
                    return true;
                });
            var vm = CreateViewModel();

            await vm.RecalculateSeriesResultsCommand.ExecuteAsync(null);

            _dialogs.Received(1).ShowInformation(Arg.Is<string>(m => m.Contains("2 failed")), "Finished");
        }

        [Fact]
        public async Task RecalculateSeriesResults_ReportsCancelled_WhenProgressDialogCancels()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _dialogs.ShowProgressAsync(Arg.Any<string>(),
                Arg.Any<Func<IProgress<DownloadProgress>, CancellationToken, Task>>()).Returns(Task.FromResult(false));
            var vm = CreateViewModel();

            await vm.RecalculateSeriesResultsCommand.ExecuteAsync(null);

            _dialogs.Received(1).ShowInformation(Arg.Is<string>(m => m.Contains("cancelled")), "Cancel");
        }

        [Fact]
        public async Task ExportCommands_UseDistinctDefaultFileNames()
        {
            // The two exports must not offer each other's default name; a shared helper builds both.
            _dialogs.PickSaveFile(Arg.Any<string>(), Arg.Any<string>()).Returns((string)null);
            var vm = CreateViewModel();

            await vm.ExportRaceResultsCommand.ExecuteAsync(null);
            await vm.ExportSeriesResultsCommand.ExecuteAsync(null);

            _dialogs.Received(1).PickSaveFile(Arg.Any<string>(), Arg.Is<string>(n => n.StartsWith("races-")));
            _dialogs.Received(1).PickSaveFile(Arg.Any<string>(), Arg.Is<string>(n => n.StartsWith("series-results-")));
        }
    }
}
