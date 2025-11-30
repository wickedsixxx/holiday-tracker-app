using HolidayTrackerApp.Domain;
using HolidayTrackerApp.Domain.Enums;
using HolidayTrackerApp.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Servislerin Eklenmesi
builder.Services.AddControllers();

// --- YENÝ EKLENDÝ: CORS Politikasý ---
// Tarayýcýnýn API'ye eriþim engeli (Failed to fetch) koymamasý için gerekli izinler.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()  // Her yerden gelen isteði kabul et
                  .AllowAnyMethod()  // GET, POST, PUT, DELETE hepsine izin ver
                  .AllowAnyHeader(); // Tüm baþlýklara izin ver
        });
});
// -------------------------------------

// Connection string kontrolü
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(connectionString));

// Program.cs - builder.Services bölümüne ekle

builder.Services.AddIdentity<Employee, IdentityRole<Guid>>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

var app = builder.Build();

// 2. Veritabaný Migrasyonlarýný Otomatik Uygulama
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Veritabanýný oluþturur ve bekleyen migration'larý basar
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabaný migrasyonu sýrasýnda bir hata oluþtu.");
    }
}

// 3. Middleware Ayarlarý
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- YENÝ EKLENDÝ: CORS'u Devreye Al ---
// Bu satýr UseHttpsRedirection'dan ÖNCE olmalýdýr.
app.UseCors("AllowAll");
// ---------------------------------------

app.UseHttpsRedirection();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    // Roller yoksa oluþturulmasýný saðla
    if (!await roleManager.RoleExistsAsync(RoleName.Manager.ToString()))
    {
        await roleManager.CreateAsync(new IdentityRole<Guid>(RoleName.Manager.ToString()));
    }
    if (!await roleManager.RoleExistsAsync(RoleName.Employee.ToString()))
    {
        await roleManager.CreateAsync(new IdentityRole<Guid>(RoleName.Employee.ToString()));
    }
}

app.Run();