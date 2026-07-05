using System;
using System.Collections.Generic;
using System.Data;
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

        public Boat Get(int bid)
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
            using (var ctx = _contextFactory.CreateDbContext())
            {
                var conn = ctx.Database.GetDbConnection();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT IDENT_CURRENT('boats')";
                    var mustOpen = conn.State != ConnectionState.Open;
                    if (mustOpen) conn.Open();
                    try
                    {
                        var result = cmd.ExecuteScalar();
                        return Convert.ToInt32(result) + 1;
                    }
                    finally
                    {
                        if (mustOpen) conn.Close();
                    }
                }
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
