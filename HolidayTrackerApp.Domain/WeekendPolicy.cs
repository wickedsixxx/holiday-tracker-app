using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HolidayTrackerApp.Domain.Entities;


namespace HolidayTrackerApp.Domain;

public sealed class WeekendPolicy
{
    public Guid Id { get; set; }
    public ICollection<WeekendDay> WeekendDays { get; set; } = new List<WeekendDay>();
}
