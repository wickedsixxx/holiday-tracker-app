using HolidayTrackerApp.Domain;
using HolidayTrackerApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HolidayTrackerApp.Infrastructure
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<WeekendPolicy> WeekendPolicies { get; set; }
        public DbSet<WeekendDay> WeekendDays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                 .HasIndex(e => e.EmployeeNo)
                  .IsUnique();

            modelBuilder.Entity<LeaveBalance>()
                   .HasKey(l => new { l.EmployeeId, l.Year });

            modelBuilder.Entity<Holiday>()
                  .HasIndex(h => new { h.Year, h.Date })
                  .IsUnique();


            // WeekendDay tablosu ve ilişkisi
            modelBuilder.Entity<WeekendDay>(e =>
{
    e.ToTable("WeekendDays");
    e.HasKey(x => x.Id);

    // Day (DayOfWeek) enum'unu int olarak sakla
    e.Property(x => x.Day).HasConversion<int>();

    // İlişki: WeekendPolicy 1 — N WeekendDay
    e.HasOne(x => x.WeekendPolicy)
     .WithMany(p => p.WeekendDays)
     .HasForeignKey(x => x.WeekendPolicyId)
     .OnDelete(DeleteBehavior.Cascade);

    // Aynı policy’de aynı gün bir kez olsun
    e.HasIndex(x => new { x.WeekendPolicyId, x.Day }).IsUnique();
});



            base.OnModelCreating(modelBuilder);
        }
    }


}
