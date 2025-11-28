using Microsoft.EntityFrameworkCore;
using WhatsAppBookingService.Data;
using WhatsAppBookingService.Models;

namespace WhatsAppBookingService.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(ApplicationDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> GetOrCreateUserAsync(string phoneNumber, string? name)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user == null)
            {
                user = new User
                {
                    PhoneNumber = phoneNumber,
                    Name = name,
                    CreatedAt = DateTime.UtcNow,
                    LastContact = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created new user: {PhoneNumber}", phoneNumber);
            }
            else
            {
                user.LastContact = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(user.Name))
                {
                    user.Name = name;
                }
                await _context.SaveChangesAsync();
            }

            return user;
        }

        public async Task<List<TimeOnly>> GetAvailableTimeSlotsAsync(DateOnly date)
        {
            // Get business hours from config
            var startHourConfig = await _context.BusinessConfigs.FirstOrDefaultAsync(c => c.ConfigKey == "business_start_hour");
            var endHourConfig = await _context.BusinessConfigs.FirstOrDefaultAsync(c => c.ConfigKey == "business_end_hour");
            var slotDurationConfig = await _context.BusinessConfigs.FirstOrDefaultAsync(c => c.ConfigKey == "slot_duration_minutes");

            int startHour = int.Parse(startHourConfig?.ConfigValue ?? "9");
            int endHour = int.Parse(endHourConfig?.ConfigValue ?? "18");
            int slotDuration = int.Parse(slotDurationConfig?.ConfigValue ?? "60");

            // Generate all possible time slots
            var allSlots = new List<TimeOnly>();
            var currentTime = new TimeOnly(startHour, 0);
            var endTime = new TimeOnly(endHour, 0);

            while (currentTime < endTime)
            {
                allSlots.Add(currentTime);
                currentTime = currentTime.AddMinutes(slotDuration);
            }

            // Get booked appointments for this date
            var bookedTimes = await _context.Appointments
                .Where(a => a.AppointmentDate == date && a.Status != "cancelled")
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            // Return only available slots
            return allSlots.Where(slot => !bookedTimes.Contains(slot)).ToList();
        }

        public async Task<Appointment?> CreateAppointmentAsync(int userId, DateOnly date, TimeOnly time, string? serviceType)
        {
            try
            {
                // Check if slot is still available
                var existingAppointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentDate == date && a.AppointmentTime == time && a.Status != "cancelled");

                if (existingAppointment != null)
                {
                    _logger.LogWarning("Time slot already booked: {Date} {Time}", date, time);
                    return null;
                }

                var appointment = new Appointment
                {
                    UserId = userId,
                    AppointmentDate = date,
                    AppointmentTime = time,
                    ServiceType = serviceType,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created appointment for user {UserId} on {Date} at {Time}", userId, date, time);
                return appointment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create appointment for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> CancelAppointmentAsync(int userId, int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId);

                if (appointment == null)
                {
                    _logger.LogWarning("Appointment not found: {AppointmentId} for user {UserId}", appointmentId, userId);
                    return false;
                }

                appointment.Status = "cancelled";
                appointment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Cancelled appointment {AppointmentId} for user {UserId}", appointmentId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel appointment {AppointmentId}", appointmentId);
                return false;
            }
        }

        public async Task<List<Appointment>> GetUserAppointmentsAsync(int userId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId && a.Status != "cancelled")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }
    }
}

