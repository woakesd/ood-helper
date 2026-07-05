using System;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class HandicapEditViewModelTests
    {
        private readonly IPortsmouthNumberRepository _repo = Substitute.For<IPortsmouthNumberRepository>();

        [Fact]
        public void NewClass_HasEmptyDefaults()
        {
            var vm = new HandicapEditViewModel(_repo, null);

            Assert.Null(vm.ClassName);
            Assert.Null(vm.Number);
            _repo.DidNotReceive().Get(Arg.Any<Guid>());
        }

        [Fact]
        public void ExistingClass_LoadsFields()
        {
            var id = Guid.NewGuid();
            _repo.Get(id).Returns(new PortsmouthNumber
            {
                Id = id, ClassName = "Laser", Number = 1100, Status = "P", NoOfCrew = 1
            });

            var vm = new HandicapEditViewModel(_repo, id);

            Assert.Equal("Laser", vm.ClassName);
            Assert.Equal(1100, vm.Number);
            Assert.Equal("P", vm.Status);
            Assert.Equal(1, vm.NoOfCrew);
        }

        [Fact]
        public void Save_PersistsClassAndRequestsClose()
        {
            PortsmouthNumber saved = null;
            _repo.When(r => r.Save(Arg.Any<PortsmouthNumber>())).Do(ci => saved = ci.Arg<PortsmouthNumber>());
            var vm = new HandicapEditViewModel(_repo, null) { ClassName = "Topper", Number = 1369 };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SaveCommand.Execute(null);

            Assert.NotNull(saved);
            Assert.Equal("Topper", saved.ClassName);
            Assert.Equal(1369, saved.Number);
            Assert.True(closed);
        }

        [Fact]
        public void Cancel_RequestsCloseWithoutSaving()
        {
            var vm = new HandicapEditViewModel(_repo, null);
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.CancelCommand.Execute(null);

            Assert.False(closed);
            _repo.DidNotReceive().Save(Arg.Any<PortsmouthNumber>());
        }
    }
}
