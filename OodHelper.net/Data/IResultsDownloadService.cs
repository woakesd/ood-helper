using System;
using System.Threading;
using System.Threading.Tasks;

namespace OodHelper.Data
{
    /// <summary>A single progress update from a download: a 0-100 percentage and a status message.</summary>
    public readonly record struct DownloadProgress(int Percent, string Message);

    /// <summary>
    /// Replaces the entire local results database with a fresh copy pulled from the club website's
    /// MySQL database. Reads stay on raw <c>MySqlConnector</c> (there is no MySQL EF provider); every
    /// write to the local SQL Server database goes through EF Core bulk insert inside one transaction.
    /// </summary>
    public interface IResultsDownloadService
    {
        Task DownloadAsync(IProgress<DownloadProgress> progress, CancellationToken ct);
    }
}
