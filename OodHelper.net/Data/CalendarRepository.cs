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
    }
}
