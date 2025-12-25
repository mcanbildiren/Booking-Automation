using BookingAPI.Data;
using BookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPI.Repositories
{
    public class WorkerRepository : Repository<Worker>, IWorkerRepository
    {
        public WorkerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Worker>> GetActiveWorkersAsync()
        {
            return await _dbSet
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<Worker?> GetWorkerWithSchedulesAsync(int id)
        {
            return await _dbSet
                .Include(w => w.Schedules)
                .FirstOrDefaultAsync(w => w.Id == id);
        }
    }
}

