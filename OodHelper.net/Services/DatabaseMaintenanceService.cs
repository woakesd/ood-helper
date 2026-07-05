using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data;

namespace OodHelper.Services
{
    internal sealed class DatabaseMaintenanceService : IDatabaseMaintenanceService
    {
        private readonly IDbContextFactory<OodHelperContext> _factory;

        public DatabaseMaintenanceService(IDbContextFactory<OodHelperContext> factory)
        {
            _factory = factory;
        }

        //
        // Realign the boats identity counter into the reserved seed band (Settings.BottomSeed/TopSeed)
        // after a full website download has inserted boats with the website's own ids. boats is the only
        // AUTOINCREMENT table that needs this: calendar/series use plain rowids, so SQLite already
        // continues from MAX(id)+1 with no intervention.
        //
        public void Reseed()
        {
            int b = OodHelper.Settings.BottomSeed;
            int t = OodHelper.Settings.TopSeed;
            if (b >= t || b == 0 || t == 0)
                return;

            using var ctx = _factory.CreateDbContext();
            int? maxInBand = ctx.Boats
                .Where(x => x.Bid >= b && x.Bid <= t)
                .Max(x => (int?)x.Bid);
            // Mirror the old DBCC CHECKIDENT reseed: set the "last used" value to seedValue so the next
            // inserted boat gets seedValue + 1.
            int seedValue = maxInBand.HasValue ? maxInBand.Value + 1 : b;

            // sqlite_sequence has no unique index, so upsert the boats row by delete-then-insert.
            ctx.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name = 'boats'");
            ctx.Database.ExecuteSql($"INSERT INTO sqlite_sequence(name, seq) VALUES('boats', {seedValue})");
        }

        //
        // Rebuild the local database from the code-first model. The existing file is backed up alongside
        // it, deleted, then recreated by applying the EF migrations.
        //
        public void RecreateDatabase()
        {
            if (File.Exists(SqliteConfig.DataFileName))
                File.Copy(SqliteConfig.DataFileName, SqliteConfig.DataFileName + ".bak", overwrite: true);

            using var ctx = _factory.CreateDbContext();
            ctx.Database.EnsureDeleted();
            ctx.Database.Migrate();
        }
    }
}
