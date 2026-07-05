using System;
using System.Threading;
using System.Threading.Tasks;

namespace OodHelper.Data
{
    /// <summary>
    /// Replaces the club website's MySQL results database with a fresh copy of the local SQL Server
    /// database. The mirror image of <see cref="IResultsDownloadService"/>: the local reads go through
    /// EF Core, every write to the website goes through raw <c>MySqlConnector</c> (there is no MySQL EF
    /// provider) inside one transaction, so any failure or cancellation leaves the website untouched.
    /// Progress is reported with the shared <see cref="DownloadProgress"/> record.
    /// </summary>
    public interface IResultsUploadService
    {
        Task UploadAsync(IProgress<DownloadProgress> progress, CancellationToken ct);
    }
}
