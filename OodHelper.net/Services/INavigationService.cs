using System.Threading.Tasks;

namespace OodHelper.Services
{
    public interface INavigationService
    {
        void OpenRaceResults(int[] rids);

        /// <summary>
        /// Scores the series behind the progress dialog, then opens its results tab. Async because the
        /// scoring pass can take a while and runs off the UI thread.
        /// </summary>
        Task OpenSeriesResultsAsync(int sid);
    }
}
