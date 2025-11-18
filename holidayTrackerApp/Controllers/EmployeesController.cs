using Microsoft.AspNetCore.Mvc;
using HolidayTrackerApp.Infrastructure;
using HolidayTrackerApp.Domain;
using Microsoft.EntityFrameworkCore;
using HolidayTrackerApp.Domain.Entities; // Eğer WeekendDay buradaysa

namespace HolidayTrackerApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Yeni Çalışan Ekleme
        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            // ID'yi boş gönderirlerse biz atayalım
            if (employee.Id == Guid.Empty)
                employee.Id = Guid.NewGuid();

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return Ok(employee);
        }

        // 2. Tüm Çalışanları Listele (LeavePolicy detaylarıyla)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Employees.Include(e => e.LeavePolicy).ToListAsync());
        }

        // 3. Potansiyel Tatil Hakkı Hesaplama
        // Örn: api/Employees/GUID-ID-BURAYA/calculate-potential/2024
        [HttpGet("{employeeId}/calculate-potential/{year}")]
        public async Task<IActionResult> CalculateYearlyPotential(Guid employeeId, int year)
        {
            var employee = await _context.Employees
                .Include(e => e.LeavePolicy)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null) return NotFound("Çalışan bulunamadı.");

            // A. Yıllık İzin Hakkını Belirle
            int paidLeaveRight = 0;

            // Önce o yıla ait LeaveBalance tablosuna bak
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.Year == year);

            if (balance != null)
            {
                paidLeaveRight = balance.PaidLeaveEarned;
            }
            else if (employee.LeavePolicy != null)
            {
                // Balance yoksa genel politikadan al
                paidLeaveRight = employee.LeavePolicy.AnnualLeaveDays;
            }

            // B. O yılın Resmi Tatillerini çek
            var holidays = await _context.Holidays
                .Where(h => h.Year == year)
                .ToListAsync();

            // C. Hafta sonu günlerini hesapla (Varsayılan: Cmt-Paz)
            List<DayOfWeek> weekendDays = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };

            int totalWeekendDays = 0;
            double uniqueHolidayDays = 0; // Yarım günler için ondalıklı sayı

            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = new DateTime(year, 12, 31);

            // Yılın her gününü tek tek kontrol et
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                bool isWeekend = weekendDays.Contains(date.DayOfWeek);

                if (isWeekend)
                {
                    totalWeekendDays++;
                }
                else
                {
                    // Hafta içi günündeyiz, bugün resmi tatil mi?
                    var holidayToday = holidays.FirstOrDefault(h => h.Date.Date == date.Date);
                    if (holidayToday != null)
                    {
                        // Yarım günse 0.5, tam günse 1.0 ekle
                        uniqueHolidayDays += holidayToday.HalfDay ? 0.5 : 1.0;
                    }
                }
            }

            // D. Sonuç Toplamı
            double grandTotal = totalWeekendDays + uniqueHolidayDays + paidLeaveRight;

            return Ok(new
            {
                EmployeeName = $"{employee.FirstName} {employee.Surname}",
                Year = year,
                Result = new
                {
                    WeekendDays = totalWeekendDays,
                    PublicHolidaysOnWorkDays = uniqueHolidayDays, // Hafta sonuna denk gelmeyen tatiller
                    AnnualPaidLeave = paidLeaveRight,
                    TotalPotentialOffDays = grandTotal
                }
            });
        }
    }
}