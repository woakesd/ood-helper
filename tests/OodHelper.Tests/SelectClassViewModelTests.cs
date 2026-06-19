using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class SelectClassViewModelTests
    {
        private readonly IPortsmouthNumberRepository _repo = Substitute.For<IPortsmouthNumberRepository>();

        private static IReadOnlyList<PortsmouthNumber> Items(params (Guid Id, string Name)[] items)
        {
            return items.Select(i => new PortsmouthNumber { Id = i.Id, ClassName = i.Name }).ToList();
        }

        [Fact]
        public void Load_PopulatesRows()
        {
            _repo.GetAll(string.Empty).Returns(Items((Guid.NewGuid(), "Laser"), (Guid.NewGuid(), "Topper")));
            var vm = new SelectClassViewModel(_repo) { DebounceMilliseconds = 0 };

            vm.Load();

            Assert.NotNull(vm.Rows);
            Assert.Equal(2, vm.Rows.Count);
        }

        [Fact]
        public void Select_WithSelection_RequestsCloseTrueAndExposesId()
        {
            var id = Guid.NewGuid();
            var vm = new SelectClassViewModel(_repo) { DebounceMilliseconds = 0 };
            vm.SelectedRow = new PortsmouthNumber { Id = id, ClassName = "Laser" };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SelectCommand.Execute(null);

            Assert.True(closed);
            Assert.Equal(id, vm.SelectedId);
        }

        [Fact]
        public void Select_WithoutSelection_RequestsCloseFalse()
        {
            var vm = new SelectClassViewModel(_repo) { DebounceMilliseconds = 0 };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SelectCommand.Execute(null);

            Assert.False(closed);
            Assert.Null(vm.SelectedId);
        }

        [Fact]
        public void Cancel_RequestsCloseFalse()
        {
            var vm = new SelectClassViewModel(_repo) { DebounceMilliseconds = 0 };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.CancelCommand.Execute(null);

            Assert.False(closed);
        }
    }
}
