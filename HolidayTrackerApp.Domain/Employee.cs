using Microsoft.AspNetCore.Identity; // <-- Bu kütüphaneyi ekledik

namespace HolidayTrackerApp.Domain;

// IdentityUser<Guid> diyerek hem Email, Password gibi alanları hazır alıyoruz
// Hem de ID'nin Guid olacağını söylüyoruz.
public sealed class Employee : IdentityUser<Guid>
{
    // public Guid Id { get; set; } <-- BUNU SİLDİK! (Çünkü IdentityUser'ın içinde zaten Id var, çakışmasın)

    public string EmployeeNo { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? MiddleName { get; set; }
    public string Surname { get; set; } = "";
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int AnnualLeaveQuota { get; set; }
    public Guid LeavePolicyId { get; set; }
    public LeavePolicy? LeavePolicy { get; set; }
}