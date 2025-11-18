using HolidayTrackerApp.Domain;
using HolidayTrackerApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolidayTrackerApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeaveRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. İzin Talebi Oluştur (POST: api/LeaveRequests)
        [HttpPost]
        public async Task<IActionResult> CreateRequest(LeaveRequest request)
        {
            // A. Tarih Kontrolü
            if (request.StartDate > request.EndDate)
                return BadRequest("Başlangıç tarihi bitiş tarihinden sonra olamaz.");

            var employee = await _context.Employees
                .Include(e => e.LeavePolicy)
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeID);

            if (employee == null) return NotFound("Çalışan bulunamadı.");

            // B. Talep Edilen Gün Sayısını Hesapla (Hafta sonu ve Resmi Tatil Düşülmüş)
            double requestedDays = await CalculateBusinessDays(request.StartDate, request.EndDate);

            if (requestedDays <= 0)
                return BadRequest("Seçilen tarih aralığında iş günü bulunmuyor.");

            // C. Bakiye Kontrolü
            int currentYear = request.StartDate.Year;
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == request.EmployeeID && b.Year == currentYear);

            int currentRemaining = 0;
            if (balance != null)
            {
                currentRemaining = balance.Remaining;
            }
            else if (employee.LeavePolicy != null)
            {
                currentRemaining = employee.LeavePolicy.AnnualLeaveDays;
            }

            double projectedBalance = currentRemaining - requestedDays;

            // D. Eksi Bakiye ve Politika Kontrolü
            if (projectedBalance < 0)
            {
                // Politika yoksa veya eksiye izin vermiyorsa
                if (employee.LeavePolicy == null || !employee.LeavePolicy.AllowNegativeBalance)
                {
                    return BadRequest($"Yetersiz bakiye! Mevcut Hakkınız: {currentRemaining} gün. Talep: {requestedDays} gün.");
                }

                // Eksi limit kontrolü
                if (Math.Abs(projectedBalance) > employee.LeavePolicy.MaxNegativeDays)
                {
                    return BadRequest($"Eksi bakiye limiti aşılıyor! Maksimum eksiye düşme hakkınız: {employee.LeavePolicy.MaxNegativeDays} gün.");
                }
            }

            // E. Kayıt İşlemi
            // ID client'tan gelmediyse oluştur
            if (request.Id == Guid.Empty) request.Id = Guid.NewGuid();

            request.Status = LeaveStatus.Pending;

            // NOT: Entity'nizde 'RequestDays' olmadığı için veritabanına gün sayısını kaydetmiyoruz.
            // Sadece Başlangıç ve Bitiş tarihleri kaydediliyor.

            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "İzin talebi oluşturuldu.",
                RequestedDays = requestedDays,
                NewProjectedBalance = projectedBalance,
                RequestDetails = request
            });
        }

        // 2. İzin Onaylama / Reddetme (POST: api/LeaveRequests/{id}/approve?approved=true)
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveRequest(Guid id, [FromQuery] bool approved)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound("İzin talebi bulunamadı.");
            if (request.Status != LeaveStatus.Pending) return BadRequest("Bu talep zaten işlem görmüş.");

            // REDDEDİLME DURUMU
            if (!approved)
            {
                request.Status = LeaveStatus.Rejected;
                await _context.SaveChangesAsync();
                return Ok(new { Message = "İzin talebi reddedildi." });
            }

            // ONAYLANMA DURUMU
            request.Status = LeaveStatus.Approved;

            // Bakiyeden Düşme İşlemi
            int year = request.StartDate.Year;
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == request.EmployeeID && b.Year == year);

            // Eğer çalışanın o yıl için bakiye kaydı henüz yoksa oluştur
            if (balance == null)
            {
                var policy = await _context.LeavePolicies.FindAsync(request.Employee!.LeavePolicyId);
                int initialRight = policy?.AnnualLeaveDays ?? 14;

                balance = new LeaveBalance
                {
                    EmployeeId = request.EmployeeID,
                    Year = year,
                    PaidLeaveEarned = initialRight,
                    PaidLeaveUsed = 0,
                    PaidLeaveDebt = 0
                };
                _context.LeaveBalances.Add(balance);
            }

            // Düşülecek gün sayısını tekrar hesapla (Veritabanında gün sayısı tutmadığımız için tekrar hesaplıyoruz)
            double daysToDeduct = await CalculateBusinessDays(request.StartDate, request.EndDate);

            // Bakiyeden düş (Kullanılanı artır)
            // Not: PaidLeaveUsed integer olduğu için yukarı yuvarlayarak (Ceiling) tam sayıya çeviriyoruz.
            balance.PaidLeaveUsed += (int)Math.Ceiling(daysToDeduct);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "İzin onaylandı ve bakiyeden düşüldü.", CurrentRemaining = balance.Remaining });
        }

        // Yardımcı Metod: Net İş Günü Hesaplama
        private async Task<double> CalculateBusinessDays(DateTime start, DateTime end)
        {
            var holidays = await _context.Holidays
                .Where(h => h.Year >= start.Year && h.Year <= end.Year)
                .ToListAsync();

            double businessDays = 0;
            List<DayOfWeek> weekendDays = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (weekendDays.Contains(date.DayOfWeek)) continue; // Hafta sonuysa sayma

                var holiday = holidays.FirstOrDefault(h => h.Date.Date == date.Date);
                if (holiday != null)
                {
                    if (holiday.IsWorkdayOverride) businessDays += 1;      // Tatil ama çalışılıyor
                    else if (holiday.HalfDay) businessDays += 0.5;         // Yarım gün
                    // Tam gün tatilse (HalfDay=false) hiç ekleme yapma (0)
                }
                else
                {
                    businessDays += 1; // Normal iş günü
                }
            }
            return businessDays;
        }
    }
}