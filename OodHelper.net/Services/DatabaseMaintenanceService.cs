namespace OodHelper.Services
{
    internal sealed class DatabaseMaintenanceService : IDatabaseMaintenanceService
    {
        public void Reseed()
        {
            Db.ReseedDatabase();
        }

        public void RecreateDatabase()
        {
            Db.CreateDb();
        }
    }
}
