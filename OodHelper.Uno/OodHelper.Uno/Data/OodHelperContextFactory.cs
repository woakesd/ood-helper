using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OodHelper.Data
{
    /// <summary>
    /// Design-time factory used by the EF Core tools (e.g. <c>dotnet ef migrations</c>) to construct the
    /// context without spinning up the WPF <c>App</c> dependency-injection graph. Runtime construction goes
    /// through <c>App.xaml.cs</c>'s <c>AddDbContextFactory</c> instead.
    /// </summary>
    internal sealed class OodHelperContextFactory : IDesignTimeDbContextFactory<OodHelperContext>
    {
        public OodHelperContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<OodHelperContext>()
                .UseSqlite(SqliteConfig.ConnectionString)
                .Options;
            return new OodHelperContext(options);
        }
    }
}
