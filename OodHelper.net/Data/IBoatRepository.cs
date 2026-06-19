using System.Collections.Generic;
using OodHelper.Data.Entities;

namespace OodHelper.Data
{
    public interface IBoatRepository
    {
        IReadOnlyList<Boat> Search(string filter);
        void Delete(int bid);
    }
}
