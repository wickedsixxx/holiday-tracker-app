using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolidayTrackerApp.Domain.Entities
{
    public sealed class WeekendDay
    {
        public int Id { get; set; }
        public Guid WeekendPolicyId { get; set; }
        public DayOfWeek Day { get; set; }

        // navigation property
        public WeekendPolicy? WeekendPolicy { get; set; }
    }
}
