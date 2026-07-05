using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OodHelper.Data;
using OodHelper.Results.Scoring;

namespace OodHelper.Results
{
    /// <summary>
    /// Host view-model for the Series Results screen. Replaces the old <c>RaceSeriesResult</c>
    /// BackgroundWorker: <see cref="Build"/> re-scores each race in the series, totals the standings
    /// per class via the pure <see cref="SeriesResult"/> algorithm, persists them and builds one
    /// <see cref="SeriesDisplayViewModel"/> per class. It is driven off the UI thread behind the
    /// progress dialog by <c>NavigationService.OpenSeriesResultsAsync</c>; all data access goes through
    /// <see cref="ISeriesResultRepository"/> / <see cref="IRaceScoreRepository"/> (no <c>Db</c>).
    /// </summary>
    public sealed class SeriesResultsViewModel
    {
        private readonly ISeriesResultRepository _repo;
        private readonly IRaceScoreRepository _scoreRepo;

        public SeriesResultsViewModel(ISeriesResultRepository repo, IRaceScoreRepository scoreRepo)
        {
            _repo = repo;
            _scoreRepo = scoreRepo;
        }

        public string SeriesName { get; private set; }

        public IReadOnlyList<SeriesDisplayViewModel> Displays { get; private set; } =
            new List<SeriesDisplayViewModel>();

        /// <summary>
        /// Runs the full scoring/totalling pass for one series. Synchronous and self-contained so it
        /// can run on the thread pool behind the progress dialog; reports progress and honours
        /// cancellation between races.
        /// </summary>
        public void Build(int sid, IProgress<DownloadProgress> progress, CancellationToken ct)
        {
            var header = _repo.GetSeriesHeader(sid);
            var seriesName = header?.Name;
            var seriesDiscards = header?.Discards;

            //
            // 1. (Re)score every race in the series, exactly as the old screen did before totalling.
            //
            var races = _repo.GetRacesToScore(sid);
            for (int i = 0; i < races.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var race = races[i];
                if (!Enum.TryParse<CalendarModel.RaceTypes>(race.RaceType, out var raceType))
                    continue;

                progress?.Report(new DownloadProgress(Percent(i, races.Count),
                    "Calculating " + race.EventName + " - " + race.ClassName));

                IRaceScore scorer = null;
                switch (raceType)
                {
                    case CalendarModel.RaceTypes.AverageLap:
                    case CalendarModel.RaceTypes.FixedLength:
                    case CalendarModel.RaceTypes.TimeGate:
                    case CalendarModel.RaceTypes.HybridOld:
                        switch ((race.Handicapping ?? string.Empty).ToUpper())
                        {
                            case "R": scorer = new HandicapScorer(_scoreRepo, HandicapMode.Rolling); break;
                            case "O": scorer = new HandicapScorer(_scoreRepo, HandicapMode.Open); break;
                        }
                        break;
                }

                scorer?.Calculate(race.Rid);
            }

            //
            // 2. Read every result row and bucket by class into per-event entries.
            //
            var seriesData = BuildSeriesData(_repo.GetEntryRows(sid));

            //
            // 3. Score each class, persist the standings and build its display.
            //
            var displays = new List<SeriesDisplayViewModel>();
            foreach (var className in seriesData.Keys.ToList())
            {
                ct.ThrowIfCancellationRequested();
                var classData = seriesData[className];

                // Drop events that nobody finished (the old NumberOfFinishers == 0 prune).
                foreach (var rid in classData.Where(kv => kv.Value.NumberOfFinishers == 0)
                             .Select(kv => kv.Key).ToList())
                    classData.Remove(rid);

                // If the prune empties the class there is nothing to score. The old code would have
                // thrown on the empty discard-profile index here; skipping is the safe equivalent.
                if (classData.Count == 0)
                    continue;

                var sr = new SeriesResult(sid, className, classData, ParseDiscardProfile(seriesDiscards));
                progress?.Report(new DownloadProgress(100, "Calculating series " + className));
                sr.Score();
                sr.SeriesName = seriesName + " - " + className;

                SaveResults(sid, className, sr);
                displays.Add(BuildDisplay(sr));
            }

            SeriesName = seriesName;
            Displays = displays;
        }

        private static Dictionary<string, Dictionary<int, SeriesEvent>> BuildSeriesData(
            IReadOnlyList<SeriesEntryRow> entryRows)
        {
            var seriesData = new Dictionary<string, Dictionary<int, SeriesEvent>>();
            foreach (var re in entryRows)
            {
                if (!seriesData.TryGetValue(re.ClassName, out var classData))
                {
                    classData = new Dictionary<int, SeriesEvent>();
                    seriesData[re.ClassName] = classData;
                }
                if (!classData.TryGetValue(re.Rid, out var ev))
                {
                    ev = new SeriesEvent(re.Rid, re.StartDate);
                    classData[re.Rid] = ev;
                }

                ev.AddEntry(new SeriesEntry
                {
                    bid = re.Bid,
                    rid = ev.Rid,
                    code = (re.FinishCode ?? string.Empty).ToUpper().Trim(),
                    date = re.StartDate,
                    points = re.Points,
                    override_points = re.OverridePoints
                });
            }
            return seriesData;
        }

        private void SaveResults(int sid, string division, SeriesResult sr)
        {
            try
            {
                var rows = sr.Results
                    .Select(b => new SeriesResultRow(b.Bid, b.Count, b.Total, b.Net, b.Place))
                    .ToList();
                _repo.SaveSeriesResults(sid, division, rows);
            }
            catch (Exception e)
            {
                // Matches the old SeriesResult.SaveResults: a persistence failure is logged, not thrown.
                ErrorLogger.LogException(e);
            }
        }

        private SeriesDisplayViewModel BuildDisplay(SeriesResult sr)
        {
            var boats = _repo.GetBoats(sr.Results.Select(b => b.Bid).ToList());

            var rows = sr.Results.Select(bsr =>
            {
                boats.TryGetValue(bsr.Bid, out var bd);
                return new SeriesRowViewModel
                {
                    Bid = bsr.Bid,
                    Boatname = bd?.Boatname,
                    Boatclass = bd?.Boatclass,
                    Sailno = bd?.Sailno,
                    Entered = bsr.Count,
                    Place = bsr.Place,
                    Score = bsr.Net,
                    Cells = bsr.DateSortedPoints
                };
            }).ToList();

            // One race column per (kept) event, matching the old r1..rn columns.
            return new SeriesDisplayViewModel(sr.SeriesName, sr.Division, sr.Results.Count,
                sr.SeriesData.Count, rows);
        }

        private static int Percent(int index, int count) =>
            count <= 0 ? 0 : (int)(index * 100L / count);

        private static int[] ParseDiscardProfile(string seriesDiscards)
        {
            var discards = seriesDiscards;
            if (string.IsNullOrEmpty(discards))
            {
                discards = Settings.DefaultDiscardProfile;
                if (string.IsNullOrEmpty(discards))
                    discards = "0,1";
            }

            var parts = discards.Split(',');
            var profile = new int[parts.Length];
            try
            {
                for (int i = 0; i < parts.Length; i++)
                    profile[i] = int.Parse(parts[i]);
            }
            catch
            {
                profile = new[] { 0, 1 };
            }
            return profile;
        }
    }
}
