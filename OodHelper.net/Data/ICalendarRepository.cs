using System.Collections.Generic;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    /// <summary>
    /// EF Core data access for race (calendar) maintenance: the <c>calendar</c> table behind the
    /// Races list and the per-race editor.
    /// </summary>
    public interface ICalendarRepository
    {
        /// <summary>All calendar rows ordered by date, optionally filtered by event name.</summary>
        IReadOnlyList<Calendar> GetAll(string filter);
        Calendar Get(int rid);
        /// <summary>Insert or update a calendar row. Returns the (possibly new) rid.</summary>
        int Save(Calendar calendar);
        void Delete(int rid);
        /// <summary>Updates only the memo (race notes) for one race, leaving every other column untouched.</summary>
        void UpdateMemo(int rid, string memo);
    }
}
