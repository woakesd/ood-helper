using System;
using System.Threading.Tasks;
using OodHelper.Results;

namespace OodHelper.Services
{
    internal sealed class NavigationService : INavigationService
    {
        private readonly ITabHost _tabHost;
        private readonly IDialogService _dialogs;
        private readonly Func<int[], RaceResults> _raceResultsFactory;
        private readonly Func<SeriesResultsViewModel> _seriesResultsFactory;

        public NavigationService(ITabHost tabHost, IDialogService dialogs,
            Func<int[], RaceResults> raceResultsFactory,
            Func<SeriesResultsViewModel> seriesResultsFactory)
        {
            _tabHost = tabHost;
            _dialogs = dialogs;
            _raceResultsFactory = raceResultsFactory;
            _seriesResultsFactory = seriesResultsFactory;
        }

        public void OpenRaceResults(int[] rids)
        {
            _tabHost.AddTab("Race Results", _raceResultsFactory(rids));
        }

        public async Task OpenSeriesResultsAsync(int sid)
        {
            //
            // The heavy scoring/totalling runs on the thread pool behind the progress dialog (replaces
            // the old RaceSeriesResult BackgroundWorker + Working + UI delegate). On completion we are
            // back on the UI thread, so the tab/view can be created here.
            //
            var vm = _seriesResultsFactory();
            var completed = await _dialogs.ShowProgressAsync("Calculating series results",
                (progress, ct) => Task.Run(() => vm.Build(sid, progress, ct), ct));

            if (completed)
                _tabHost.AddTab("Series Result", new SeriesDisplayByClass(vm));
        }
    }
}
