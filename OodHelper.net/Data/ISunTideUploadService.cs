using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OodHelper.Data
{
    /// <summary>
    /// Uploads computed sun (sunrise/sunset) and parsed tide data to the club website's MySQL database.
    /// Unlike the results upload, the source here is an in-memory <see cref="DataTable"/> built by the
    /// sun/tide views rather than the local database, so these take the table directly. Each upload is a
    /// date-range replace (delete the covered dates, then insert the supplied rows) inside one MySQL
    /// transaction; cancellation or failure rolls back and leaves the website untouched.
    /// </summary>
    public interface ISunTideUploadService
    {
        Task UploadSunAsync(DataTable sun, IProgress<DownloadProgress> progress, CancellationToken ct);
        Task UploadTideAsync(DataTable tide, IProgress<DownloadProgress> progress, CancellationToken ct);
    }
}
