using System;
using OodHelper;
using OodHelper.Data.Entities;
using OodHelper.Results;
using Xunit;

namespace OodHelper.Tests
{
    public class ResultRowViewModelTests
    {
        private static ResultRowViewModel Row(Race race, out Box<Race> persisted)
        {
            var box = new Box<Race>();
            persisted = box;
            return new ResultRowViewModel(race, new DateTime(2024, 1, 1), r => box.Value = r);
        }

        // Small mutable holder so tests can observe the persistence callback.
        private sealed class Box<T> { public T Value; }

        [Fact]
        public void RestrictedSail_Checked_AppliesCoefficientAndPersists()
        {
            var race = new Race { Rid = 1, Bid = 2, OpenHandicap = 1000, RollingHandicap = 900 };
            var row = Row(race, out var persisted);

            row.RestrictedSail = true;

            Assert.Equal((int) Math.Round(1000 * Settings.RSCoefficieent), race.OpenHandicap);
            Assert.Equal((int) Math.Round(900 * Settings.RSCoefficieent), race.RollingHandicap);
            Assert.True(race.RestrictedSail);
            Assert.Same(race, persisted.Value);
        }

        [Fact]
        public void RestrictedSail_Unchecked_RemovesCoefficientAndClearsFlag()
        {
            var inflated = (int) Math.Round(1000 * Settings.RSCoefficieent);
            var race = new Race { Rid = 1, Bid = 2, RestrictedSail = true, OpenHandicap = inflated };
            var row = Row(race, out var persisted);

            row.RestrictedSail = false;

            Assert.Equal((int) Math.Round(inflated / Settings.RSCoefficieent), race.OpenHandicap);
            Assert.Null(race.RestrictedSail);
            Assert.Same(race, persisted.Value);
        }

        [Fact]
        public void EditingFinishCode_UpdatesEntityAndPersists()
        {
            var race = new Race { Rid = 1, Bid = 2 };
            var row = Row(race, out var persisted);

            row.FinishCode = "DNF";

            Assert.Equal("DNF", race.FinishCode);
            Assert.Same(race, persisted.Value);
        }

        [Fact]
        public void SettingStartTime_CombinesWithCalendarDateAndPersists()
        {
            var race = new Race { Rid = 1, Bid = 2 };
            var row = Row(race, out var persisted);

            row.StartTime = "10:30:00";

            Assert.True(race.StartDate.HasValue);
            Assert.Equal(new TimeSpan(10, 30, 0), race.StartDate.Value.TimeOfDay);
            Assert.Equal(new DateTime(2024, 1, 1), race.StartDate.Value.Date);
            Assert.Same(race, persisted.Value);
        }

        [Fact]
        public void ReadOnlyFormatting_ElapsedRendersAsHms()
        {
            var race = new Race { Rid = 1, Bid = 2, Elapsed = 3661 }; // 1h 01m 01s
            var row = Row(race, out _);

            Assert.Equal("01:01:01", row.Elapsed);
        }
    }
}
