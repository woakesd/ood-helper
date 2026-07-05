using MySqlConnector;
using NSubstitute;
using OodHelper.Services;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class ConfigurationViewModelTests
    {
        private readonly ISettingsService _settings = Substitute.For<ISettingsService>();
        private readonly IDatabaseMaintenanceService _dbMaintenance = Substitute.For<IDatabaseMaintenanceService>();

        public ConfigurationViewModelTests()
        {
            _settings.BottomSeed.Returns(1000);
            _settings.TopSeed.Returns(1999);
            _settings.RhCoefficient.Returns(0.1);
            _settings.RsCoefficient.Returns(1.03);
            _settings.Mysql.Returns("Server=db.example.com;User ID=ood;Password=secret;Database=results;Port=3307;Use Compression=true;SSL Mode=Required");
            _settings.DefaultDiscardProfile.Returns(string.Empty);
            _settings.PusherAppId.Returns("app-id");
            _settings.PusherAppKey.Returns("app-key");
            _settings.PusherAppSecret.Returns("app-secret");
        }

        [Fact]
        public void Constructor_ParsesMysqlConnectionStringIntoFields()
        {
            var vm = new ConfigurationViewModel(_settings, _dbMaintenance);

            Assert.Equal("db.example.com", vm.Server);
            Assert.Equal("ood", vm.Username);
            Assert.Equal("secret", vm.Password);
            Assert.Equal("results", vm.Database);
            Assert.Equal("3307", vm.Port);
            Assert.True(vm.UseCompression);
            Assert.Equal(ConfigurationViewModel.SslRequired, vm.SslModeIndex);
        }

        [Fact]
        public void Constructor_DefaultsEmptyDiscardProfile()
        {
            var vm = new ConfigurationViewModel(_settings, _dbMaintenance);

            Assert.Equal("0,1", vm.DefaultDiscardProfile);
        }

        [Fact]
        public void Save_WritesSettingsReseedsAndRequestsClose()
        {
            var vm = new ConfigurationViewModel(_settings, _dbMaintenance);
            bool? closeResult = null;
            vm.CloseRequested += result => closeResult = result;

            vm.BottomSeed = "2000";
            vm.TopSeed = "2999";
            vm.RhCoefficient = "0.2";
            vm.RsCoefficient = "1.05";
            vm.SaveCommand.Execute(null);

            var mcsb = new MySqlConnectionStringBuilder(_settings.Mysql);
            _settings.Received().Mysql = Arg.Is<string>(cs =>
                new MySqlConnectionStringBuilder(cs).Server == "db.example.com" &&
                new MySqlConnectionStringBuilder(cs).Port == 3307 &&
                new MySqlConnectionStringBuilder(cs).SslMode == MySqlSslMode.Required);
            _settings.Received().BottomSeed = 2000;
            _settings.Received().TopSeed = 2999;
            _settings.Received().RhCoefficient = 0.2;
            _settings.Received().RsCoefficient = 1.05;
            _settings.Received().DefaultDiscardProfile = "0,1";
            _settings.Received().PusherAppId = "app-id";
            _dbMaintenance.Received(1).Reseed();
            Assert.True(closeResult);
        }

        [Fact]
        public void Save_SkipsInvalidNumericInputs()
        {
            var vm = new ConfigurationViewModel(_settings, _dbMaintenance);

            vm.BottomSeed = "not-a-number";
            vm.TopSeed = "0";
            vm.RhCoefficient = "abc";
            vm.SaveCommand.Execute(null);

            _settings.DidNotReceive().BottomSeed = Arg.Any<int>();
            _settings.DidNotReceive().TopSeed = Arg.Any<int>();
            _settings.DidNotReceive().RhCoefficient = Arg.Any<double>();
            _settings.Received().RsCoefficient = 1.03;
            _dbMaintenance.Received(1).Reseed();
        }

        [Fact]
        public void ClearSeeds_EmptiesSeedFields()
        {
            var vm = new ConfigurationViewModel(_settings, _dbMaintenance);

            vm.ClearSeedsCommand.Execute(null);

            Assert.Equal(string.Empty, vm.BottomSeed);
            Assert.Equal(string.Empty, vm.TopSeed);
        }
    }
}
