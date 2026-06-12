using NSubstitute;
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

        private OodHelperWindowViewModel CreateViewModel()
        {
            return new OodHelperWindowViewModel(_dialogs, _navigation, _dbMaintenance);
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
        public void Download_DoesNothing_WhenNotConfirmed()
        {
            _dialogs.Confirm(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
            var vm = CreateViewModel();

            vm.DownloadCommand.Execute(null);

            _dialogs.Received(1).Confirm(Arg.Any<string>(), "Confirm Download");
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
