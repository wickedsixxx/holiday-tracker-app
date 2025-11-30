using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Linq;

// 👇 Bu satırlar kritik, hepsinin olduğundan emin ol:
using HolidayTrackerApp.Domain;                 // Employee için
using HolidayTrackerApp.Domain.Enums;           // RoleName için
using HolidayTrackerApp.Domain.Entities;        // Bazen entityler burada olabilir
using HolidayTrackerApp.Application.DTOs;       // LoginDto, RegisterRequest için
using HolidayTrackerApp.Application.Interfaces; // IAuthenticationService için
using HolidayTrackerApp.Infrastructure;    // AppDbContext için (Genelde buradadır)
// Eğer Data klasörü yoksa şunu dene: using HolidayTrackerApp.Infrastructure;

namespace HolidayTrackerApp.Api.Controllers
{
    // Rota: /api/Auth
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Employee> _userManager;
        private readonly SignInManager<Employee> _signInManager;
        private readonly AppDbContext _context;
        private readonly IAuthenticationService _authenticationService;

        public AuthController(
            UserManager<Employee> userManager,
            SignInManager<Employee> signInManager,
            AppDbContext context,
            IAuthenticationService authenticationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _authenticationService = authenticationService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "Email ve Şifre zorunludur." });
            }

            // Veritabanında bir politika olup olmadığını kontrol et (Program.cs'te ekleme yapsak da kontrol edelim)
            var policy = await _context.LeavePolicies.FirstOrDefaultAsync();
            if (policy == null)
            {
                return StatusCode(500, new { Message = "Sistemde tanımlı izin politikası bulunamadı. Lütfen bir yönetici ile görüşün." });
            }

            var employee = new Employee
            {
                UserName = request.Email,
                Email = request.Email,
                EmployeeNo = "TEMP" + Guid.NewGuid().ToString().Substring(0, 4), // Geçici çalışan no
                FirstName = "Yeni",
                Surname = "Kullanıcı",
                HireDate = DateTime.Today,
                AnnualLeaveQuota = policy.AnnualLeaveDays,
                LeavePolicyId = policy.Id
            };

            var result = await _userManager.CreateAsync(employee, request.Password);

            if (result.Succeeded)
            {
                // 1. Rolü Belirle (String olarak bir değişkende tutalım)
                string roleName = RoleName.Employee.ToString(); // Varsayılan olarak Employee

                // Eğer sistemdeki ilk kullanıcı ise onu Manager (Yönetici) yapalım
                if (!_userManager.Users.Any(u => u.Id != employee.Id))
                {
                    roleName = RoleName.Manager.ToString();
                }

                // 2. Rolü Veritabanına İşle (Identity sistemi bunu 'AspNetUserRoles' tablosuna yazar)
                await _userManager.AddToRoleAsync(employee, roleName);

                // ❌ SİLDİĞİMİZ KISIM: employee.Role = ... (Buna gerek yok, hata veren yer burasıydı)

                // 3. Token Oluştur (Değişkendeki 'roleName'i kullanıyoruz)
                // Burada artık employee.Role değil, yukarıda belirlediğimiz roleName değişkenini veriyoruz.
                var token = _authenticationService.CreateToken(employee.Id.ToString(), employee.Email, roleName);

                return Ok(new { Token = token, Message = "Kayıt başarılı, giriş yapıldı." });
            }

            return BadRequest(new { Message = "Kayıt işlemi başarısız.", Errors = result.Errors });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var employee = await _userManager.FindByEmailAsync(request.Email);
            if (employee == null || !await _userManager.CheckPasswordAsync(employee, request.Password))
            {
                return Unauthorized(new { Message = "Geçersiz email veya şifre." });
            }

            // Çalışanın rolünü al
            var roles = await _userManager.GetRolesAsync(employee);
            var role = roles.FirstOrDefault(); // Varsayılan olarak ilk rolü alıyoruz

            if (string.IsNullOrEmpty(role))
            {
                role = RoleName.Employee.ToString(); // Rolü yoksa varsayılan çalışan rolü
            }

            // Token oluştur ve döndür
            var token = _authenticationService.CreateToken(employee.Id.ToString(), employee.Email, role);

            return Ok(new { Token = token, Message = "Giriş başarılı." });
        }
    }
}

// DTO'lar (Data Transfer Objects) - Normalde ayrı dosyada olmalılar
public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);