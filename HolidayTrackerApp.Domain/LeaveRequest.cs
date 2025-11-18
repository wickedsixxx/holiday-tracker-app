namespace HolidayTrackerApp.Domain
{
    public sealed class LeaveRequest
    {
        public Guid Id { get; set; }
        public Guid EmployeeID { get; set; }
        public LeaveType LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        public int LeaveRange => (EndDate - StartDate).Days + 1;
    }

}

