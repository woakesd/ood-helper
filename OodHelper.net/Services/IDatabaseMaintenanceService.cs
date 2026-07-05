namespace OodHelper.Services
{
    public interface IDatabaseMaintenanceService
    {
        void Reseed();
        void RecreateDatabase();
    }
}
