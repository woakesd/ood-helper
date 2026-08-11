using System;
using System.Collections.Generic;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    /// <summary>A calendar race shown in the read-only member list of a series.</summary>
    public sealed record SeriesRaceItem(int Rid, string? Event, string? EventClass, DateTime? StartDate);

    /// <summary>A calendar race in the race-select grid, with its current series membership.</summary>
    public sealed record SeriesRaceCandidate(int Rid, string? Event, string? EventClass, DateTime? StartDate, bool Selected);

    public interface ISeriesRepository
    {
        IReadOnlyList<Series> GetAll(string filter);
        Series? Get(int sid);
        /// <summary>Insert or update a series. Returns the (possibly new) sid.</summary>
        int Save(Series series);
        void Delete(int sid);
        /// <summary>The races that belong to the series, ordered by date.</summary>
        IReadOnlyList<SeriesRaceItem> GetMemberRaces(int sid);
        /// <summary>All calendar races, ordered by date, each flagged with its series membership.</summary>
        IReadOnlyList<SeriesRaceCandidate> GetAllRacesWithMembership(int sid);
        /// <summary>Replace the series' race membership with the given race ids.</summary>
        void SetMemberRaces(int sid, IEnumerable<int> rids);
    }
}
