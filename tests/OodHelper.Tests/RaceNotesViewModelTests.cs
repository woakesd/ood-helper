using NSubstitute;
using OodHelper.Data;
using OodHelper.Data.Entities;
using OodHelper.ViewModels;
using Xunit;

namespace OodHelper.Tests
{
    public class RaceNotesViewModelTests
    {
        private readonly ICalendarRepository _repo = Substitute.For<ICalendarRepository>();

        [Fact]
        public void Ctor_LoadsEventClassAndMemo()
        {
            _repo.Get(7).Returns(new Calendar { Rid = 7, Event = "Spring Series", Class = "Fast", Memo = "windy" });

            var vm = new RaceNotesViewModel(_repo, 7);

            Assert.Equal("Spring Series", vm.EventName);
            Assert.Equal("Fast", vm.ClassName);
            Assert.Equal("windy", vm.Memo);
        }

        [Fact]
        public void Ctor_UnknownRace_LeavesFieldsNull()
        {
            _repo.Get(Arg.Any<int>()).Returns((Calendar)null);

            var vm = new RaceNotesViewModel(_repo, 7);

            Assert.Null(vm.EventName);
            Assert.Null(vm.ClassName);
            Assert.Null(vm.Memo);
        }

        [Fact]
        public void Save_PersistsEditedMemoAndRequestsClose()
        {
            _repo.Get(7).Returns(new Calendar { Rid = 7, Memo = "old" });
            var vm = new RaceNotesViewModel(_repo, 7) { Memo = "new note" };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.SaveCommand.Execute(null);

            _repo.Received(1).UpdateMemo(7, "new note");
            Assert.True(closed);
        }

        [Fact]
        public void Cancel_RequestsCloseWithoutSaving()
        {
            _repo.Get(7).Returns(new Calendar { Rid = 7, Memo = "old" });
            var vm = new RaceNotesViewModel(_repo, 7) { Memo = "edited but discarded" };
            bool? closed = null;
            vm.CloseRequested += r => closed = r;

            vm.CancelCommand.Execute(null);

            Assert.False(closed);
            _repo.DidNotReceive().UpdateMemo(Arg.Any<int>(), Arg.Any<string>());
        }
    }
}
