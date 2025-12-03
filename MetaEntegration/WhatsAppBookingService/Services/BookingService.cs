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

        #region Worker Methods

        public async Task<List<Worker>> GetActiveWorkersAsync()
        {
            return await _context.Workers
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<Worker?> GetWorkerByIdAsync(int workerId)
        {
            return await _context.Workers
                .Include(w => w.Schedules)
                .FirstOrDefaultAsync(w => w.Id == workerId && w.IsActive);
        }

        #endregion

        #region Availability Methods

        public async Task<List<TimeOnly>> GetAvailableTimeSlotsForWorkerAsync(int workerId, DateOnly date)
        {
            // Get the worker's schedule for this day of the week
            int dayOfWeek = (int)date.DayOfWeek;
            
            var workerSchedule = await _context.WorkerSchedules
                .FirstOrDefaultAsync(ws => ws.WorkerId == workerId && ws.DayOfWeek == dayOfWeek && ws.IsWorking);

            // If worker doesn't work on this day, return empty list
            if (workerSchedule == null)
            {
                _logger.LogInformation("Worker {WorkerId} is not working on {DayOfWeek}", workerId, date.DayOfWeek);
                return new List<TimeOnly>();
            }

            // Get slot duration from config (default 60 minutes)
            var slotDurationConfig = await _context.BusinessConfigs.FirstOrDefaultAsync(c => c.ConfigKey == "slot_duration_minutes");
            int slotDuration = int.Parse(slotDurationConfig?.ConfigValue ?? "60");

            // Generate all possible time slots based on worker's schedule
            var allSlots = new List<TimeOnly>();
            var currentTime = workerSchedule.StartTime;
            var endTime = workerSchedule.EndTime;

            while (currentTime < endTime)
            {
                allSlots.Add(currentTime);
                currentTime = currentTime.AddMinutes(slotDuration);
            }

            // Get booked appointments for this worker on this date
            var bookedTimes = await _context.Appointments
                .Where(a => a.WorkerId == workerId && a.AppointmentDate == date && a.Status != "cancelled")
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            // Return only available slots
            return allSlots.Where(slot => !bookedTimes.Contains(slot)).ToList();
        }

        #endregion

        #region Appointment Methods

        public async Task<Appointment?> CreateAppointmentAsync(int userId, int workerId, DateOnly date, TimeOnly time, string? serviceType)
        {
            try
            {
                // Check if slot is still available for this worker
                var existingAppointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.WorkerId == workerId && a.AppointmentDate == date && a.AppointmentTime == time && a.Status != "cancelled");

                if (existingAppointment != null)
                {
                    _logger.LogWarning("Time slot already booked for worker {WorkerId}: {Date} {Time}", workerId, date, time);
                    return null;
                }

                var appointment = new Appointment
                {
                    UserId = userId,
                    WorkerId = workerId,
                    AppointmentDate = date,
                    AppointmentTime = time,
                    ServiceType = serviceType,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created appointment for user {UserId} with worker {WorkerId} on {Date} at {Time}", userId, workerId, date, time);
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
                .Include(a => a.Worker)
                .Where(a => a.UserId == userId && a.Status != "cancelled")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        #endregion
    }
}

