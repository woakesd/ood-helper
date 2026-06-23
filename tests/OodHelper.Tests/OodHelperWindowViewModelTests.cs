using System;
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

        private OodHelperWindowViewModel CreateViewModel()
        {
            return new OodHelperWindowViewModel(_dialogs, _navigation, _dbMaintenance, _download, _upload,
                _updateCheck);
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
        public void ShowSeriesResults_OpensTab_WhenChooserReturnsSeries()
        {
            _dialogs.ShowSeriesChooser().Returns(5);
            var vm = CreateViewModel();

            vm.ShowSeriesResultsCommand.Execute(null);

            _navigation.Received(1).OpenSeriesResults(5);
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
    }
}
