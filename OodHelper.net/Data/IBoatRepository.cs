using System.Collections.Generic;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    public interface IBoatRepository
    {
        IReadOnlyList<Boat> Search(string filter);

        /// <summary>
        /// As <see cref="Search(string)"/>, but optionally restricts to dinghies (<c>true</c>) or
        /// yachts (<c>false</c>); <c>null</c> applies no dinghy filter.
        /// </summary>
        IReadOnlyList<Boat> Search(string filter, bool? dinghy);

        Boat Get(int bid);

        /// <summary>Inserts (when <c>Bid == 0</c>) or updates the boat.</summary>
        void Save(Boat boat);

        /// <summary>
        /// The identity value the next inserted boat will receive (<c>IDENT_CURRENT + 1</c>).
        /// Used to validate a new boat against the configured seed range before it is added.
        /// </summary>
        int GetNextIdentity();

        void Delete(int bid);
    }
}
