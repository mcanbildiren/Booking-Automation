using BookingAPI.Models;

namespace BookingAPI.Repositories
{
    public interface IWorkerRepository : IRepository<Worker>
    {
        Task<IEnumerable<Worker>> GetActiveWorkersAsync();
        Task<Worker?> GetWorkerWithSchedulesAsync(int id);
    }
}

