using System;
using System.Linq;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    /// <summary>
    /// Seeds a little demo data on first run. The Uno app owns its <em>own</em> SQLite file (separate
    /// from the WPF app's), so after the initial migration it is empty; this makes the walking-skeleton
    /// screens (handicaps list, class picker, series editor + race picker) demonstrable with real rows
    /// without depending on an existing production database. It is a no-op once data is present.
    /// </summary>
    internal static class DevDataSeeder
    {
        public static void Seed(OodHelperContext ctx)
        {
            if (!ctx.PortsmouthNumbers.Any())
            {
                ctx.PortsmouthNumbers.AddRange(
                    NewClass("Laser", 1100),
                    NewClass("Laser Radial", 1145),
                    NewClass("RS Aero 7", 1065),
                    NewClass("Topper", 1365),
                    NewClass("Optimist", 1642),
                    NewClass("Solo", 1142),
                    NewClass("Firefly", 1173),
                    NewClass("GP14", 1130));
                ctx.SaveChanges();
            }

            if (!ctx.Calendars.Any())
            {
                var start = new DateTime(2026, 4, 1);
                for (int i = 0; i < 6; i++)
                {
                    ctx.Calendars.Add(new Calendar
                    {
                        Rid = 1001 + i,
                        Event = $"Spring Series Race {i + 1}",
                        Class = "Handicap",
                        StartDate = start.AddDays(i * 7),
                        IsRace = true,
                        Raced = false,
                    });
                }
                ctx.SaveChanges();
            }

            if (!ctx.Series.Any())
            {
                // Attach the first two seeded races so the series has member races and the race
                // picker shows some already-selected.
                var firstRaces = ctx.Calendars.OrderBy(c => c.StartDate).Take(2).ToList();
                var series = new Series { Sname = "Demo Spring Series 2026", Discards = "1" };
                foreach (var r in firstRaces)
                    series.Rids.Add(r);
                ctx.Series.Add(series);
                ctx.SaveChanges();
            }
        }

        private static PortsmouthNumber NewClass(string name, int number) => new PortsmouthNumber
        {
            Id = Guid.NewGuid(),
            ClassName = name,
            Number = number,
            NoOfCrew = 1,
        };
    }
}
