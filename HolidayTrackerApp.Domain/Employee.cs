namespace HolidayTrackerApp.Domain;

public sealed class Employee
{

    public Guid Id { get; set; }
    public string EmployeeNo { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? MiddleName { get; set; }
    public string Surname { get; set; } = "";
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;


    public Guid LeavePolicyId { get; set; }
    public LeavePolicy? LeavePolicy { get; set; }
}