using System;
using OodHelper.Results;

namespace OodHelper.Services
{
    internal sealed class NavigationService : INavigationService
    {
        private readonly ITabHost _tabHost;

        public NavigationService(ITabHost tabHost)
        {
            _tabHost = tabHost;
        }

        public void OpenRaceResults(int[] rids)
        {
            _tabHost.AddTab("Race Results", new RaceResults(rids));
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
