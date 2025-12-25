using BookingAPI.Models;

namespace BookingAPI.Repositories
{
    public interface IWorkerScheduleRepository : IRepository<WorkerSchedule>
    {
        Task<WorkerSchedule?> GetByWorkerAndDayAsync(int workerId, int dayOfWeek);
        Task<IEnumerable<WorkerSchedule>> GetByWorkerIdAsync(int workerId);
    }
}

