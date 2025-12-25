using BookingAPI.Models;

namespace BookingAPI.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    }
}

