using WhatsAppBookingService.Models;

namespace WhatsAppBookingService.Services
{
    public interface IBookingService
    {
        Task<User> GetOrCreateUserAsync(string phoneNumber, string? name);
        
        // Worker methods
        Task<List<Worker>> GetActiveWorkersAsync();
        Task<Worker?> GetWorkerByIdAsync(int workerId);
        
        // Availability methods - now worker-specific
        Task<List<TimeOnly>> GetAvailableTimeSlotsForWorkerAsync(int workerId, DateOnly date);
        
        // Appointment methods
        Task<Appointment?> CreateAppointmentAsync(int userId, int workerId, DateOnly date, TimeOnly time, string? serviceType);
        Task<bool> CancelAppointmentAsync(int userId, int appointmentId);
        Task<List<Appointment>> GetUserAppointmentsAsync(int userId);
    }
}

