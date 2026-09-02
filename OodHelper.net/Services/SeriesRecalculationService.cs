using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OodHelper.Data;
using OodHelper.Results;

namespace OodHelper.Services
{
    /// <summary>Outcome of a bulk series recalculation.</summary>
    /// <param name="Recalculated">Series that were re-totalled and had their standings replaced.</param>
    /// <param name="SkippedNoResults">Series with no raced races to total, left untouched.</param>
    /// <param name="Failed">
    /// Series that threw, or whose new standings could not be written; logged and stepped over.
    /// </param>
    public sealed record SeriesRecalculationSummary(int Recalculated, int SkippedNoResults, int Failed);

    public interface ISeriesRecalculationService
    {
        /// <summary>
        /// Re-totals every series that has results available, replacing its stored standings. Race
        /// results are left exactly as they are - no race is re-scored and no rolling handicap is
        /// rewritten; only the series totals are rebuilt from the stored race points. Series with
        /// nothing to total are left untouched.
        /// </summary>
        Task<SeriesRecalculationSummary> RecalculateAllAsync(IProgress<DownloadProgress> progress,
            CancellationToken ct);
    }

    internal sealed class SeriesRecalculationService : ISeriesRecalculationService
    {
        private readonly ISeriesRepository _series;
        private readonly ISeriesResultRepository _seriesResults;
        private readonly Func<SeriesResultsViewModel> _resultsFactory;

        public SeriesRecalculationService(ISeriesRepository series, ISeriesResultRepository seriesResults,
            Func<SeriesResultsViewModel> resultsFactory)
        {
            _series = series;
            _seriesResults = seriesResults;
            _resultsFactory = resultsFactory;
        }

        //
        // The scan below costs a query per series, so it gets the first slice of the progress bar and
        // the recalculation itself gets the rest; without it the dialog sits at 0% through the scan.
        //
        private const int ScanPercent = 10;

        //
        // Synchronous work wrapped in a completed Task, matching IResultsDownloadService: the caller
        // (DialogService.ShowProgressAsync) already pushes this onto the thread pool.
        //
        public Task<SeriesRecalculationSummary> RecalculateAllAsync(IProgress<DownloadProgress> progress,
            CancellationToken ct)
        {
            //
            // "Results available" means the series has at least one raced race carrying race rows.
            // Series without any are skipped rather than scored to an empty standing, so their stored
            // results are left alone. Because the per-series pass below does not re-score races, this
            // is the only GetRacesToScore query each series pays for.
            //
            var all = _series.GetAll(string.Empty);
            var todo = new List<(int Sid, string Name)>();
            for (int i = 0; i < all.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var s = all[i];
                progress?.Report(new DownloadProgress((int)(i * (long)ScanPercent / all.Count),
                    $"{s.Sname}: checking for results"));
                if (_seriesResults.GetRacesToScore(s.Sid).Count > 0)
                    todo.Add((s.Sid, s.Sname));
            }

            //
            // Reuse the same pass the Series Results screen runs for one series - minus the race
            // re-scoring - so the bulk totals cannot drift from what the screen produces for the same
            // race results. Race points and rolling handicaps are read, never rewritten.
            //
            int recalculated = 0, failed = 0;
            for (int i = 0; i < todo.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var (sid, name) = todo[i];
                var slice = new SeriesSliceProgress(progress, name, i, todo.Count);
                try
                {
                    // Without the per-race loop the only progress a series reports is per class, so
                    // announce the series itself to keep the dialog moving through the run.
                    slice.Report(new DownloadProgress(0, "Totalling series results"));
                    var results = _resultsFactory();
                    results.Build(sid, slice, ct, rescoreRaces: false);

                    //
                    // Build logs persistence failures rather than throwing, so a series can come back
                    // with standings that were computed but never written. That is a failed series as
                    // far as this run is concerned - the stored results did not change.
                    //
                    if (results.SaveFailures > 0)
                        failed++;
                    else
                        recalculated++;
                }
                catch (OperationCanceledException)
                {
                    // Cancellation is not a per-series failure; let it abort the run.
                    throw;
                }
                catch (Exception ex)
                {
                    // One unscoreable series must not abandon the rest of the run.
                    ErrorLogger.LogException(ex);
                    failed++;
                }
            }

            return Task.FromResult(
                new SeriesRecalculationSummary(recalculated, all.Count - todo.Count, failed));
        }

        //
        // Rescales one series' 0-100 progress into its slice of the run's post-scan range and prefixes
        // the series name, so the dialog advances steadily instead of restarting at 0% for every series.
        //
        private sealed class SeriesSliceProgress : IProgress<DownloadProgress>
        {
            private readonly IProgress<DownloadProgress>? _inner;
            private readonly string _name;
            private readonly int _index;
            private readonly int _count;

            public SeriesSliceProgress(IProgress<DownloadProgress>? inner, string name, int index, int count)
            {
                _inner = inner;
                _name = name;
                _index = index;
                _count = count;
            }

            public void Report(DownloadProgress value)
            {
                if (_inner == null || _count <= 0)
                    return;
                var percent = Math.Clamp(value.Percent, 0, 100);
                var overall = ScanPercent +
                    (int)((_index * 100L + percent) * (100 - ScanPercent) / (_count * 100L));
                _inner.Report(new DownloadProgress(overall, $"{_name}: {value.Message}"));
            }
        }
    }
}
