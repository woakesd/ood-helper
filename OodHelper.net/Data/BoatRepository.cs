using System.Collections;
using System.Data;

namespace OodHelper.Data
{
    internal sealed class BoatRepository : IBoatRepository
    {
        public DataTable Search(string filter)
        {
            using (Db c = new Db(@"SELECT *
FROM boats
WHERE boatname LIKE @filter
or sailno LIKE @filter
or boatclass LIKE @filter
ORDER BY boatname"))
            {
                Hashtable p = new Hashtable();
                p["filter"] = string.Format("%{0}%", filter);
                return c.GetData(p);
            }
        }

        public void Delete(int bid)
        {
            using (Db del = new Db("DELETE FROM boats WHERE bid = @bid"))
            {
                Hashtable p = new Hashtable();
                p["bid"] = bid;
                del.ExecuteNonQuery(p);
            }
        }
    }
}
