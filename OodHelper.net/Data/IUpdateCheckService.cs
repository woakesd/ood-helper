using System;
using System.Threading;
using System.Threading.Tasks;

namespace OodHelper.Data
{
    /// <summary>
    /// The most recent results-upload timestamp on each side. <see cref="WebsiteIsNewer"/> mirrors the
    /// legacy CheckForUpdates test (LocalDate &lt; RemoteDate): it is true only when both sides have an
    /// upload recorded and the website's is the more recent.
    /// </summary>
    public readonly record struct UpdateCheckResult(DateTime? LocalDate, DateTime? RemoteDate)
    {
        public bool WebsiteIsNewer => LocalDate < RemoteDate;
    }

    /// <summary>
    /// Compares the latest results-upload time in the local SQL Server database against the club
    /// website's MySQL database, so the app can offer a download at startup when the website is ahead.
    /// The local read goes through EF; the remote read is a single raw <c>MySqlConnector</c> scalar
    /// (there is no MySQL EF provider).
    /// </summary>
    public interface IUpdateCheckService
    {
        Task<UpdateCheckResult> CheckAsync(CancellationToken ct);
    }
}
