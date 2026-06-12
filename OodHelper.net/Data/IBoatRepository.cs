using System.Data;

namespace OodHelper.Data
{
    public interface IBoatRepository
    {
        DataTable Search(string filter);
        void Delete(int bid);
    }
}
