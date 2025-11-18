using Microsoft.AspNetCore.Mvc;
using HolidayTrackerApp.Infrastructure;
using HolidayTrackerApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace HolidayTrackerApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeavePoliciesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeavePoliciesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Politika Ekleme (POST: api/LeavePolicies)
        [HttpPost]
        public async Task<IActionResult> Create(LeavePolicy policy)
        {
            // ID boş gelirse yeni oluştur
            if (policy.Id == Guid.Empty) policy.Id = Guid.NewGuid();

            _context.LeavePolicies.Add(policy);
            await _context.SaveChangesAsync();
            return Ok(policy);
        }

        // 2. Tüm Politikaları Getir (GET: api/LeavePolicies)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.LeavePolicies.ToListAsync());
        }
    }
}