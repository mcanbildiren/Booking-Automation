using BookingAPI.Models;
using BookingAPI.Models.ViewModels;

namespace BookingAPI.Services
{
    public interface IAppointmentService
    {
        Task<AppointmentIndexViewModel> GetAppointmentsForIndexAsync(DateOnly selectedDate, int? workerId, string? status, string? search);
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<Appointment?> GetAppointmentWithDetailsAsync(int id);
        Task<Appointment> CreateAppointmentFromAdminAsync(string phoneNumber, string? customerName, int workerId, 
            DateOnly date, TimeOnly time, int durationMinutes, string status, string? serviceType, string? notes);
        Task<bool> UpdateAppointmentAsync(int id, int workerId, DateOnly date, TimeOnly time, 
            int durationMinutes, string status, string? serviceType, string? notes);
        Task<bool> DeleteAppointmentAsync(int id);
        Task<bool> HasSlotConflictAsync(int workerId, DateOnly date, TimeOnly time, int? excludeAppointmentId = null);
    }
}
