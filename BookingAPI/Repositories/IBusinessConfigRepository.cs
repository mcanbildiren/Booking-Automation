using BookingAPI.Models;

namespace BookingAPI.Repositories
{
    public interface IBusinessConfigRepository : IRepository<BusinessConfig>
    {
        Task<BusinessConfig?> GetByKeyAsync(string key);
    }
}

