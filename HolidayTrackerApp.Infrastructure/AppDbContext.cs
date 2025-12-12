using HolidayTrackerApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HolidayTrackerApp.Domain;
using System;
using System.Linq; // Query operasyonları için sıklıkla gereklidir

namespace HolidayTrackerApp.Infrastructure
{
    // AppDbContext, artık IdentityDbContext'ten miras alıyor.
    public sealed class AppDbContext : IdentityDbContext<Employee, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        
        public DbSet<Employee> Employees => base.Users;

        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<WeekendPolicy> WeekendPolicies { get; set; }
        public DbSet<WeekendDay> WeekendDays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity tablolarının doğru oluşturulması için bu satır EN BAŞTA OLMALIDIR.
            base.OnModelCreating(modelBuilder);

            // Diğer Fluent API konfigürasyonlarınız...
            modelBuilder.Entity<Employee>()
                 .HasIndex(e => e.EmployeeNo)
                 .IsUnique();

            modelBuilder.Entity<LeaveBalance>()
                .HasKey(l => new { l.EmployeeId, l.Year });

            modelBuilder.Entity<Holiday>()
                .HasIndex(h => new { h.Year, h.Date })
                .IsUnique();

            modelBuilder.Entity<WeekendDay>(e =>
            {
                e.ToTable("WeekendDays");
                e.HasKey(x => x.Id);
                e.Property(x => x.Day).HasConversion<int>();
                e.HasOne(x => x.WeekendPolicy)
                 .WithMany(p => p.WeekendDays)
                 .HasForeignKey(x => x.WeekendPolicyId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => new { x.WeekendPolicyId, x.Day }).IsUnique();
            });
        }
    }
}