using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class SeriesChooserViewModelTests
    {
        private readonly ISeriesRepository _repo = Substitute.For<ISeriesRepository>();

        private static IReadOnlyList<Series> Items(params (int Sid, string Name)[] items)
        {
            return items.Select(i => new Series { Sid = i.Sid, Sname = i.Name }).ToList();
        }

        [Fact]
        public void Load_PopulatesRows()
        {
            _repo.GetAll(string.Empty).Returns(Items((1, "Spring"), (2, "Autumn")));
            var vm = new SeriesChooserViewModel(_repo);

            vm.Load();

            Assert.NotNull(vm.Rows);
            Assert.Equal(2, vm.Rows.Count);
        }

        [Fact]
        public void Choose_WithSelection_RequestsCloseTrueAndExposesSid()
        {
            var vm = new SeriesChooserViewModel(_repo);
            vm.SelectedRow = new Series { Sid = 9, Sname = "Spring" };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.ChooseCommand.Execute(null);

            Assert.True(closed);
            Assert.Equal(9, vm.SelectedSid);
        }

        [Fact]
        public void Choose_WithoutSelection_RequestsCloseFalse()
        {
            var vm = new SeriesChooserViewModel(_repo);
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.ChooseCommand.Execute(null);

            Assert.False(closed);
            Assert.Null(vm.SelectedSid);
        }

        [Fact]
        public void Cancel_RequestsCloseFalse()
        {
            var vm = new SeriesChooserViewModel(_repo);
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.CancelCommand.Execute(null);

            Assert.False(closed);
        }
    }
}
