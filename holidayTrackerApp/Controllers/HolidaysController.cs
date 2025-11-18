using Microsoft.AspNetCore.Mvc;
using HolidayTrackerApp.Infrastructure;
using HolidayTrackerApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace HolidayTrackerApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidaysController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HolidaysController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Resmi Tatil Ekleme (POST: api/Holidays)
        [HttpPost]
        public async Task<IActionResult> Create(Holiday holiday)
        {
            // Çakışma kontrolü (Aynı tarihte kayıt var mı?)
            bool exists = await _context.Holidays.AnyAsync(h => h.Date == holiday.Date);
            if (exists) return BadRequest("Bu tarihte zaten bir tatil kaydı var.");

            // Tatilin yılı, girilen tarihin yılı olsun
            if (holiday.Year == 0) holiday.Year = holiday.Date.Year;

            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();
            return Ok(holiday);
        }

        // 2. Yıla göre tatilleri getir (GET: api/Holidays/2024)
        [HttpGet("{year}")]
        public async Task<IActionResult> GetByYear(int year)
        {
            var holidays = await _context.Holidays
                                         .Where(h => h.Year == year)
                                         .OrderBy(h => h.Date)
                                         .ToListAsync();
            return Ok(holidays);
        }

        // 3. Tatil Silme (DELETE: api/Holidays/5)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null) return NotFound();

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}