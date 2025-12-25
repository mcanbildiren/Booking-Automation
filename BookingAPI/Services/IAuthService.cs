using System.Security.Claims;

namespace BookingAPI.Services
{
    public interface IAuthService
    {
        Task<ClaimsPrincipal?> ValidateCredentialsAsync(string username, string password);
    }
}

