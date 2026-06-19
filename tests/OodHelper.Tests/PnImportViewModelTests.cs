using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.Services;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class PnImportViewModelTests
    {
        private readonly IPortsmouthNumberRepository _repo = Substitute.For<IPortsmouthNumberRepository>();
        private readonly IDialogService _dialogs = Substitute.For<IDialogService>();

        private static string WriteTempCsv()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csv");
            File.WriteAllText(path,
                "ClassName,NoOfCrew,Rig,Spinnaker,Engine,Keel,Number,Status,Notes\r\n" +
                "Laser,1,U,0,,,1100,P,fast\r\n" +
                "Topper,1,U,0,,,1369,S,\r\n");
            return path;
        }

        [Fact]
        public void Browse_ReadsCsvIntoRows()
        {
            var path = WriteTempCsv();
            try
            {
                _dialogs.PickOpenFile(Arg.Any<string>()).Returns(path);
                var vm = new PnImportViewModel(_repo, _dialogs);

                vm.BrowseCommand.Execute(null);

                Assert.Equal(path, vm.FileName);
                Assert.NotNull(vm.Rows);
                Assert.Equal(2, vm.Rows.Count);
                Assert.Equal("Laser", vm.Rows[0].ClassName);
                Assert.Equal(1100, vm.Rows[0].Number);
                Assert.Equal(1, vm.Rows[0].NoOfCrew);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Browse_WhenCancelled_LeavesRowsNull()
        {
            _dialogs.PickOpenFile(Arg.Any<string>()).Returns((string)null);
            var vm = new PnImportViewModel(_repo, _dialogs);

            vm.BrowseCommand.Execute(null);

            Assert.Null(vm.Rows);
            Assert.Null(vm.FileName);
        }

        [Fact]
        public void Import_ReplacesAllWithParsedRows()
        {
            var path = WriteTempCsv();
            try
            {
                _dialogs.PickOpenFile(Arg.Any<string>()).Returns(path);
                IEnumerable<PortsmouthNumber> imported = null;
                _repo.When(r => r.ReplaceAll(Arg.Any<IEnumerable<PortsmouthNumber>>()))
                    .Do(ci => imported = ci.Arg<IEnumerable<PortsmouthNumber>>().ToList());
                var vm = new PnImportViewModel(_repo, _dialogs);
                vm.BrowseCommand.Execute(null);

                vm.ImportCommand.Execute(null);

                Assert.NotNull(imported);
                Assert.Equal(2, imported.Count());
                Assert.Contains(imported, p => p.ClassName == "Topper");
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Import_WithNoRows_DoesNothing()
        {
            var vm = new PnImportViewModel(_repo, _dialogs);

            vm.ImportCommand.Execute(null);

            _repo.DidNotReceive().ReplaceAll(Arg.Any<IEnumerable<PortsmouthNumber>>());
        }
    }
}
