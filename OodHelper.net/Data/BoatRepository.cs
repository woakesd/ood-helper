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
            string pattern = $"%{filter}%";
            using (var ctx = _contextFactory.CreateDbContext())
            {
                return ctx.Boats
                    .Where(b => EF.Functions.Like(b.Boatname, pattern)
                                || EF.Functions.Like(b.Sailno, pattern)
                                || EF.Functions.Like(b.Boatclass, pattern))
                    .OrderBy(b => b.Boatname)
                    .ToList();
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
