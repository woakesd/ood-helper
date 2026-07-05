using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    internal sealed class BoatRepository : IBoatRepository
    {
        private readonly IDbContextFactory<OodHelperContext> _contextFactory;

        public BoatRepository(IDbContextFactory<OodHelperContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IReadOnlyList<Boat> Search(string filter)
        {
            return Search(filter, null);
        }

        public IReadOnlyList<Boat> Search(string filter, bool? dinghy)
        {
            string pattern = $"%{filter}%";
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var q = ctx.Boats
                    .Where(b => EF.Functions.Like(b.Boatname, pattern)
                                || EF.Functions.Like(b.Sailno, pattern)
                                || EF.Functions.Like(b.Boatclass, pattern));
                if (dinghy.HasValue)
                    q = q.Where(b => b.Dinghy == dinghy.Value);
                return q.OrderBy(b => b.Boatname).ToList();
            }
        }

        public Boat? Get(int bid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Boats.AsNoTracking().FirstOrDefault(b => b.Bid == bid);
            }
        }

        public void Save(Boat boat)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                //
                // Bid is an IDENTITY column. A new boat (Bid == 0) is added so the store assigns
                // the id (mirroring the old auto-identity INSERT); an existing boat is updated in
                // full. EF writes back the generated id onto the supplied entity after the insert.
                //
                if (boat.Bid == 0)
                    ctx.Boats.Add(boat);
                else
                    ctx.Boats.Update(boat);
                ctx.SaveChanges();
            }
        }

        public int GetNextIdentity()
        {
            //
            // The id the next inserted boat will receive. boats is an AUTOINCREMENT table, so the last
            // assigned id is held in sqlite_sequence (the equivalent of the old IDENT_CURRENT); the
            // reserved seed band is enforced by reseeding that row after a website download. Falls back
            // to MAX(bid) before any insert has happened (no sqlite_sequence row yet).
            //
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var seq = ctx.Database
                    .SqlQuery<long?>($"SELECT seq AS Value FROM sqlite_sequence WHERE name = 'boats'")
                    .AsEnumerable()
                    .FirstOrDefault();
                if (seq.HasValue)
                    return (int)seq.Value + 1;

                return (ctx.Boats.Max(b => (int?)b.Bid) ?? 0) + 1;
            }
        }

        public void Delete(int bid)
        {
            using (var ctx = _contextFactory.CreateDbContext())
            {
                ctx.Boats.Where(b => b.Bid == bid).ExecuteDelete();
            }
        }
    }
}
