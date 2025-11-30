namespace HolidayTrackerApp.Application.Interfaces;

public interface IAuthenticationService
{
    // Token oluşturma metodu (AuthController'da kullandığın parametrelere göre)
    string CreateToken(string userId, string email, string role);
}