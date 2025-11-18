using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HolidayTrackerApp.Infrastructure
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                // Infrastructure’dan koşulduğunda API’deki dosyaya gider:
                .UseSqlite("Data Source=../holidayTrackerApp.Api/App_Data/holiday.db")
                .Options;

            return new AppDbContext(options);
        }
    
}
