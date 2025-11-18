namespace HolidayTrackerApp.Domain
{
    public sealed class LeaveBalance
    {
        public Guid EmployeeId { get; set; }
        public int Year { get; set; }
        public int PaidLeaveEarned { get; set; }
        public int PaidLeaveUsed { get; set; }
        public int PaidLeaveDebt { get; set; }
        public int Remaining => PaidLeaveEarned - PaidLeaveUsed - PaidLeaveDebt;
    }
}
