using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolidayTrackerApp.Domain
{
    public sealed class Holiday
    {
        public int Id { get; set; }

        public int Year { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; } = "";
        public bool HalfDay { get; set; }
        public bool IsWorkdayOverride { get; set; }
    }
}
