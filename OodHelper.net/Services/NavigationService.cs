using System;
using OodHelper.Results;

namespace OodHelper.Services
{
    internal sealed class NavigationService : INavigationService
    {
        private readonly ITabHost _tabHost;
        private readonly Func<int[], RaceResults> _raceResultsFactory;

        public NavigationService(ITabHost tabHost, Func<int[], RaceResults> raceResultsFactory)
        {
            _tabHost = tabHost;
            _raceResultsFactory = raceResultsFactory;
        }

        public void OpenRaceResults(int[] rids)
        {
            _tabHost.AddTab("Race Results", _raceResultsFactory(rids));
        }

        public void OpenSeriesResults(int sid)
        {
            RaceSeriesResult rs = null;
            Action uiDelegate = delegate
            {
                _tabHost.AddTab("Series Result", new SeriesDisplayByClass(rs));
            };
            rs = new RaceSeriesResult(sid, uiDelegate);
        }
    }
}
