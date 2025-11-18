using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolidayTrackerApp.Domain
{
    public sealed class LeavePolicy
    {
        public Guid Id { get; set; }
        public int AnnualLeaveDays { get; set; }
        public bool AllowNegativeBalance { get; set; }
        public int MaxNegativeDays { get; set; }
        public bool CarryOverToNextYear { get; set; }
        public int CarryOverCap { get; set; }
        public int MinServiceDaysForPaidLeave { get; set; }
    }
}
