using HolidayTrackerApp.Domain.Entities; // Eğer Employee sınıfınız bu namespace'deyse
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HolidayTrackerApp.Domain; // Employee sınıfının bulunduğu namespace

namespace HolidayTrackerApp.Infrastructure
{
    // IdentityDbContext<TUser, TRole, TKey> formatı:
    // TUser: Employee
    // TRole: IdentityRole<Guid>
    // TKey: Guid (Employee'nin Id tipi)
    public sealed class AppDbContext : IdentityDbContext<Employee, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }

        // Employee için DbSet'i IdentityDbContext otomatik tanımladığı için yoruma alıyoruz/siliyoruz.
        // public DbSet<Employee> Employees { get; set; }

        public DbSet<LeavePolicy> LeavePolicies { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<WeekendPolicy> WeekendPolicies { get; set; }
        public DbSet<WeekendDay> WeekendDays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity tablolarının doğru oluşturulması için bu satır en başta OLMALIDIR.
            base.OnModelCreating(modelBuilder);

            // Employee tablosu için Identity'nin varsayılanları üzerine kendi benzersiz indexinizi ekleyin
            modelBuilder.Entity<Employee>()
                 .HasIndex(e => e.EmployeeNo)
                 .IsUnique();

            // Diğer Fluent API konfigürasyonlarınız...
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