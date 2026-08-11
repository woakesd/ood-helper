using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    /// <summary>
    /// EF Core data access for the Portsmouth-number class list (the <c>portsmouth_numbers</c>
    /// table). Serves the handicaps list, the class editor, the class picker, and the CSV import.
    /// </summary>
    internal sealed class PortsmouthNumberRepository : IPortsmouthNumberRepository
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public PortsmouthNumberRepository(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IReadOnlyList<PortsmouthNumber> GetAll(string filter)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                IQueryable<PortsmouthNumber> q = ctx.PortsmouthNumbers.AsNoTracking();
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string pattern = $"%{filter.Trim()}%";
                    q = q.Where(p => EF.Functions.Like(p.ClassName, pattern));
                }

                return q.OrderBy(p => p.ClassName).ToList();
            }
        }

        public PortsmouthNumber? Get(Guid id)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.PortsmouthNumbers.AsNoTracking().FirstOrDefault(p => p.Id == id);
            }
        }

        public void Save(PortsmouthNumber pn)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                PortsmouthNumber? entity = pn.Id == Guid.Empty ? null : ctx.PortsmouthNumbers.Find(pn.Id);
                if (entity == null)
                {
                    if (pn.Id == Guid.Empty) pn.Id = Guid.NewGuid();
                    entity = new PortsmouthNumber { Id = pn.Id };
                    ctx.PortsmouthNumbers.Add(entity);
                }

                CopyFields(pn, entity);
                ctx.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                ctx.PortsmouthNumbers.Where(p => p.Id == id).ExecuteDelete();
            }
        }

        public void ReplaceAll(IEnumerable<PortsmouthNumber> rows)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                ctx.PortsmouthNumbers.ExecuteDelete();
                foreach (var row in rows)
                {
                    if (row.Id == Guid.Empty) row.Id = Guid.NewGuid();
                    ctx.PortsmouthNumbers.Add(row);
                }

                ctx.SaveChanges();
                tx.Commit();
            }
        }

        private static void CopyFields(PortsmouthNumber from, PortsmouthNumber to)
        {
            to.ClassName = from.ClassName;
            to.NoOfCrew = from.NoOfCrew;
            to.Rig = from.Rig;
            to.Spinnaker = from.Spinnaker;
            to.Engine = from.Engine;
            to.Keel = from.Keel;
            to.Number = from.Number;
            to.Status = from.Status;
            to.Notes = from.Notes;
        }
    }
}
