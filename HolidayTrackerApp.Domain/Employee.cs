using Microsoft.AspNetCore.Identity; // IdentityUser<Guid> sınıfı için gerekli

namespace HolidayTrackerApp.Domain;

// Employee, IdentityUser'dan miras alıyor ve Id tipinin Guid olduğunu belirtiyor.
// Bu doğru bir yaklaşımdır.
public sealed class Employee : IdentityUser<Guid>
{
    // Id, UserName, Email, PasswordHash vb. alanları IdentityUser<Guid> sınıfından devralındı.

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