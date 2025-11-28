using WhatsAppBookingService.Models;

namespace WhatsAppBookingService.Services
{
    public interface IBookingService
    {
        Task<User> GetOrCreateUserAsync(string phoneNumber, string? name);
        Task<List<TimeOnly>> GetAvailableTimeSlotsAsync(DateOnly date);
        Task<Appointment?> CreateAppointmentAsync(int userId, DateOnly date, TimeOnly time, string? serviceType);
        Task<bool> CancelAppointmentAsync(int userId, int appointmentId);
        Task<List<Appointment>> GetUserAppointmentsAsync(int userId);
    }
}

