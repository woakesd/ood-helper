using OodHelper.Data;

namespace OodHelper.Services
{
    internal sealed class DatabaseMaintenanceService : IDatabaseMaintenanceService
    {
        public void Reseed()
        {
            DatabaseAdmin.ReseedDatabase();
        }

        public void RecreateDatabase()
        {
            DatabaseAdmin.CreateDb();
        }
    }
}
