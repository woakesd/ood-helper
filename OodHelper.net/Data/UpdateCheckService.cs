using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace OodHelper.Data
{
    internal sealed class UpdateCheckService : IUpdateCheckService
    {
        private readonly IDbContextFactory<OodHelperContext> _factory;

        public UpdateCheckService(IDbContextFactory<OodHelperContext> factory)
        {
            _factory = factory;
        }

        //
        // updates is append-only on both sides, so the latest upload is simply MAX(upload). The local
        // value is read through EF; the remote value is a single raw scalar over MySqlConnector. An
        // empty table yields SQL NULL on either side, which surfaces as a null DateTime (never newer).
        //
        public async Task<UpdateCheckResult> CheckAsync(CancellationToken ct)
        {
            await using var ctx = await _factory.CreateDbContextAsync(ct);
            var localDate = await ctx.Updates.MaxAsync(u => u.Upload, ct);

            var mysql = new MySqlConnectionStringBuilder(Settings.Mysql).ConnectionString;
            await using var con = new MySqlConnection(mysql);
            await con.OpenAsync(ct);
            await using var cmd = new MySqlCommand("SELECT MAX(upload) FROM updates", con);
            var remoteDate = await cmd.ExecuteScalarAsync(ct) as DateTime?;

            return new UpdateCheckResult(localDate, remoteDate);
        }
    }
}
