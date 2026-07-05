using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    /// <summary>
    /// EF Core data access for series maintenance (the <c>series</c> table) and race membership
    /// (the <c>calendar_series_join</c> many-to-many, edited through the <c>Series.Rids</c> skip
    /// navigation so EF maintains the join table).
    /// </summary>
    internal sealed class SeriesRepository : ISeriesRepository
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public SeriesRepository(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IReadOnlyList<Series> GetAll(string filter)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                IQueryable<Series> q = ctx.Series.AsNoTracking();
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string pattern = $"%{filter.Trim()}%";
                    q = q.Where(s => EF.Functions.Like(s.Sname, pattern));
                }

                return q.OrderBy(s => s.Sname).ToList();
            }
        }

        public Series? Get(int sid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Series.AsNoTracking().FirstOrDefault(s => s.Sid == sid);
            }
        }

        public int Save(Series series)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var discards = string.IsNullOrWhiteSpace(series.Discards) ? null : series.Discards.Trim();

                Series? entity = series.Sid == 0 ? null : ctx.Series.Find(series.Sid);
                if (entity == null)
                {
                    entity = new Series();
                    ctx.Series.Add(entity);
                }

                entity.Sname = series.Sname?.Trim() ?? string.Empty;
                entity.Discards = discards;
                ctx.SaveChanges();
                return entity.Sid;
            }
        }

        public void Delete(int sid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var entity = ctx.Series.Include(s => s.Rids).FirstOrDefault(s => s.Sid == sid);
                if (entity == null) return;

                entity.Rids.Clear();
                ctx.Series.Remove(entity);
                ctx.SaveChanges();
            }
        }

        public IReadOnlyList<SeriesRaceItem> GetMemberRaces(int sid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Series.AsNoTracking()
                    .Where(s => s.Sid == sid)
                    .SelectMany(s => s.Rids)
                    .OrderBy(c => c.StartDate)
                    .Select(c => new SeriesRaceItem(c.Rid, c.Event, c.Class, c.StartDate))
                    .ToList();
            }
        }

        public IReadOnlyList<SeriesRaceCandidate> GetAllRacesWithMembership(int sid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var memberRids = ctx.Series
                    .Where(s => s.Sid == sid)
                    .SelectMany(s => s.Rids.Select(c => c.Rid))
                    .ToHashSet();

                return ctx.Calendars.AsNoTracking()
                    .OrderBy(c => c.StartDate)
                    .Select(c => new { c.Rid, c.Event, c.Class, c.StartDate })
                    .AsEnumerable()
                    .Select(c => new SeriesRaceCandidate(c.Rid, c.Event, c.Class, c.StartDate, memberRids.Contains(c.Rid)))
                    .ToList();
            }
        }

        public void SetMemberRaces(int sid, IEnumerable<int> rids)
        {
            var ridSet = rids.ToHashSet();
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var entity = ctx.Series.Include(s => s.Rids).FirstOrDefault(s => s.Sid == sid);
                if (entity == null) return;

                entity.Rids.Clear();
                if (ridSet.Count > 0)
                {
                    var chosen = ctx.Calendars.Where(c => ridSet.Contains(c.Rid)).ToList();
                    foreach (var c in chosen)
                        entity.Rids.Add(c);
                }

                ctx.SaveChanges();
            }
        }
    }
}
