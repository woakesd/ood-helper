using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OodHelper.Data;
using OodHelper.Results;

namespace OodHelper.Services
{
    /// <summary>Outcome of a bulk series recalculation.</summary>
    /// <param name="Recalculated">Series that were re-scored and had their standings replaced.</param>
    /// <param name="SkippedNoResults">Series with no raced races to score, left untouched.</param>
    /// <param name="Failed">Series whose recalculation threw; logged and stepped over.</param>
    public sealed record SeriesRecalculationSummary(int Recalculated, int SkippedNoResults, int Failed);

    public interface ISeriesRecalculationService
    {
        /// <summary>
        /// Re-scores and re-totals every series that has results available, replacing its stored
        /// standings. Series with nothing to score are left untouched.
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
        // Synchronous work wrapped in a completed Task, matching IResultsDownloadService: the caller
        // (DialogService.ShowProgressAsync) already pushes this onto the thread pool.
        //
        public Task<SeriesRecalculationSummary> RecalculateAllAsync(IProgress<DownloadProgress> progress,
            CancellationToken ct)
        {
            //
            // "Results available" means the series has at least one raced race carrying race rows -
            // exactly the set SeriesResultsViewModel.Build would re-score. Series without any are
            // skipped rather than scored to an empty standing, so their stored results are left alone.
            //
            var all = _series.GetAll(string.Empty);
            var todo = new List<(int Sid, string Name)>();
            foreach (var s in all)
            {
                ct.ThrowIfCancellationRequested();
                if (_seriesResults.GetRacesToScore(s.Sid).Count > 0)
                    todo.Add((s.Sid, s.Sname));
            }

            //
            // Reuse the same pass the Series Results screen runs for one series, so a bulk
            // recalculation cannot drift from what the screen produces for the same data.
            //
            int recalculated = 0, failed = 0;
            for (int i = 0; i < todo.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var (sid, name) = todo[i];
                try
                {
                    _resultsFactory().Build(sid, new SeriesSliceProgress(progress, name, i, todo.Count), ct);
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
        // Rescales one series' 0-100 progress into its slice of the overall run and prefixes the series
        // name, so the dialog advances steadily instead of restarting at 0% for every series.
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
                var overall = (int)((_index * 100L + percent) / _count);
                _inner.Report(new DownloadProgress(overall, $"{_name}: {value.Message}"));
            }
        }
    }
}
