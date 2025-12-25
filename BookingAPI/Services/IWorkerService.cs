using BookingAPI.Models;
using BookingAPI.Models.ViewModels;

namespace BookingAPI.Services
{
    public interface IWorkerService
    {
        Task<IEnumerable<WorkerListViewModel>> GetAllWorkersAsync();
        Task<IEnumerable<Worker>> GetActiveWorkersAsync();
        Task<Worker?> GetWorkerByIdAsync(int id);
        Task<Worker?> GetWorkerWithSchedulesAsync(int id);
        Task<Worker> CreateWorkerAsync(Worker worker, IEnumerable<WorkerSchedule> schedules);
        Task<bool> UpdateWorkerAsync(int id, Worker worker, IEnumerable<WorkerSchedule> schedules);
        Task<bool> ToggleWorkerActiveStatusAsync(int id);
        Task<bool> DeleteWorkerAsync(int id);
    }
}
