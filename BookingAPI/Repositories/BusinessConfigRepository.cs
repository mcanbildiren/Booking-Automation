using BookingAPI.Data;
using BookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPI.Repositories
{
    public class BusinessConfigRepository : Repository<BusinessConfig>, IBusinessConfigRepository
    {
        public BusinessConfigRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<BusinessConfig?> GetByKeyAsync(string key)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.ConfigKey == key);
        }
    }
}

