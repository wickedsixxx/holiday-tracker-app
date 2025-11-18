using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HolidayTrackerApp.Infrastructure
{
    // EF Tools için design-time factory
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=holiday.db")   // design-time için basit connection
                .Options;

            return new AppDbContext(options);
        }
    }
}
