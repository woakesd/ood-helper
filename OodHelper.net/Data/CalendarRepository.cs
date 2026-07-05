using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    /// <summary>
    /// EF Core data access for race (calendar) maintenance. Mirrors <see cref="SeriesRepository"/>:
    /// a short-lived context per operation via <see cref="IDbContextFactory{TContext}"/>.
    /// </summary>
    internal sealed class CalendarRepository : ICalendarRepository
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public CalendarRepository(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IReadOnlyList<Calendar> GetAll(string filter)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                IQueryable<Calendar> q = ctx.Calendars.AsNoTracking();
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string pattern = $"%{filter.Trim()}%";
                    q = q.Where(c => EF.Functions.Like(c.Event, pattern));
                }

                return q.OrderBy(c => c.StartDate).ToList();
            }
        }

        public Calendar Get(int rid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Calendars.AsNoTracking().FirstOrDefault(c => c.Rid == rid);
            }
        }

        public int Save(Calendar calendar)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                Calendar entity = calendar.Rid == 0 ? null : ctx.Calendars.Find(calendar.Rid);
                if (entity == null)
                {
                    entity = new Calendar();
                    ctx.Calendars.Add(entity);
                }

                //
                // Only the fields the legacy RaceEdit screen wrote are copied; SCT, result_calculated,
                // course_choice, laps_completed and the wind columns are left untouched, exactly as the
                // old INSERT/UPDATE did.
                //
                entity.StartDate = calendar.StartDate;
                entity.TimeLimitType = calendar.TimeLimitType;
                entity.TimeLimitFixed = calendar.TimeLimitFixed;
                entity.TimeLimitDelta = calendar.TimeLimitDelta;
                entity.Class = calendar.Class;
                entity.Event = calendar.Event;
                entity.PriceCode = calendar.PriceCode;
                entity.Course = calendar.Course;
                entity.Ood = calendar.Ood;
                entity.Venue = calendar.Venue;
                entity.Racetype = calendar.Racetype;
                entity.Handicapping = calendar.Handicapping;
                entity.Visitors = calendar.Visitors;
                entity.Flag = calendar.Flag;
                entity.Extension = calendar.Extension;
                entity.Memo = calendar.Memo;
                entity.Raced = calendar.Raced;
                entity.Approved = calendar.Approved;
                entity.IsRace = calendar.IsRace;

                ctx.SaveChanges();
                return entity.Rid;
            }
        }

        public void Delete(int rid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                //
                // Single-table delete mirroring the legacy `DELETE FROM calendar WHERE rid = @rid`;
                // no cascade to races / calendar_series_join, same behaviour as before.
                //
                ctx.Calendars.Where(c => c.Rid == rid).ExecuteDelete();
            }
        }

        public DateTime? GetLatestRaceDate(DateTime onOrBefore)
        {
            //
            // Mirrors the legacy `SELECT MAX(start_date) FROM calendar WHERE is_race = 1
            // AND start_date <= @today`; Max over an empty set yields null.
            //
            using (var ctx = _contextFactory.CreateDbContext())
                return ctx.Calendars.AsNoTracking()
                    .Where(c => c.IsRace == true && c.StartDate != null && c.StartDate <= onOrBefore)
                    .Max(c => c.StartDate);
        }

        public IReadOnlyList<DateTime> GetRaceDays(string filter)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                IQueryable<Calendar> q = ctx.Calendars.AsNoTracking()
                    .Where(c => c.IsRace == true && c.StartDate != null);
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string pattern = $"%{filter.Trim()}%";
                    q = q.Where(c => EF.Functions.Like(c.Event, pattern));
                }

                //
                // Distinct race days (date only), ordered. Replaces the old DATEPART
                // year/month/day grouping; EF translates .Date to CAST(... AS date).
                //
                return q.Select(c => c.StartDate.Value.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();
            }
        }

        public IReadOnlyList<Calendar> GetRacesOnDay(DateTime day)
        {
            DateTime start = day.Date;
            DateTime end = start.AddDays(1);
            using (var ctx = _contextFactory.CreateDbContext())
                return ctx.Calendars.AsNoTracking()
                    .Where(c => c.IsRace == true && c.StartDate >= start && c.StartDate < end)
                    .OrderBy(c => c.StartDate)
                    .ToList();
        }

        public IReadOnlyList<Calendar> GetUpcomingUnraced(DateTime from, int days)
        {
            //
            // Mirrors the legacy `WHERE is_race = 1 AND raced = 0 AND start_date BETWEEN @today
            // AND DATEADD(DAY, 10, @today)`; BETWEEN is inclusive, raced = 0 excludes nulls.
            //
            DateTime to = from.AddDays(days);
            using (var ctx = _contextFactory.CreateDbContext())
                return ctx.Calendars.AsNoTracking()
                    .Where(c => c.IsRace == true && c.Raced == false
                                && c.StartDate >= from && c.StartDate <= to)
                    .OrderBy(c => c.StartDate)
                    .ToList();
        }

        public void UpdateMemo(int rid, string memo)
        {
            //
            // Targeted single-column update, mirroring the legacy RaceNotes `UPDATE calendar SET memo`;
            // ExecuteUpdate leaves every other column (and the rest of the row) untouched.
            //
            using (var ctx = _contextFactory.CreateDbContext())
                ctx.Calendars.Where(c => c.Rid == rid)
                    .ExecuteUpdate(s => s.SetProperty(c => c.Memo, memo));
        }
    }
}
