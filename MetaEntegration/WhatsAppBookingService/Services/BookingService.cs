using Microsoft.EntityFrameworkCore;
using WhatsAppBookingService.Data;
using WhatsAppBookingService.Models;

namespace WhatsAppBookingService.Services
{
    public class BookingService(ApplicationDbContext context, ILogger<BookingService> logger)
        : IBookingService
    {
        private static TimeZoneInfo? _turkeyTimeZone;

        /// <summary>
        /// Gets current time in Turkey timezone (UTC+3, handles DST).
        /// Cross-platform: tries "Europe/Istanbul" (Linux/macOS) then "Turkey Standard Time" (Windows).
        /// </summary>
        private DateTime GetTurkeyTime()
        {
            if (_turkeyTimeZone == null)
            {
                try
                {
                    _turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
                }
                catch
                {
                    try
                    {
                        _turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                    }
                    catch
                    {
                        logger.LogWarning("Turkey timezone not found on this system. Using fixed UTC+3 offset.");
                        return DateTime.UtcNow.AddHours(3);
                    }
                }
            }

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _turkeyTimeZone);
        }

        public async Task<User> GetOrCreateUserAsync(string phoneNumber, string? name)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user == null)
            {
                user = new User
                {
                    PhoneNumber = phoneNumber,
                    Name = name,
                    CreatedAt = DateTime.UtcNow,
                    LastContact = DateTime.UtcNow
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();
                logger.LogInformation("Created new user: {PhoneNumber}", phoneNumber);
            }
            else
            {
                user.LastContact = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(user.Name))
                {
                    user.Name = name;
                }

                await context.SaveChangesAsync();
            }

            return user;
        }

        #region Worker Methods

        public async Task<List<Worker>> GetActiveWorkersAsync()
        {
            return await context.Workers
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<Worker?> GetWorkerByIdAsync(int workerId)
        {
            return await context.Workers
                .Include(w => w.Schedules)
                .FirstOrDefaultAsync(w => w.Id == workerId && w.IsActive);
        }

        #endregion

        #region Availability Methods

        public async Task<List<TimeOnly>> GetAvailableTimeSlotsForWorkerAsync(
            int workerId,
            DateOnly date)
        {
            int dayOfWeek = (int)date.DayOfWeek;

            var workerSchedule = await context.WorkerSchedules
                .FirstOrDefaultAsync(ws =>
                    ws.WorkerId == workerId &&
                    ws.DayOfWeek == dayOfWeek &&
                    ws.IsWorking);

            if (workerSchedule == null)
            {
                logger.LogInformation(
                    "Worker {WorkerId} is not working on {DayOfWeek}",
                    workerId,
                    date.DayOfWeek);

                return new List<TimeOnly>();
            }

            var slotDurationConfig = await context.BusinessConfigs
                .FirstOrDefaultAsync(c => c.ConfigKey == "slot_duration_minutes");

            int slotDuration = int.TryParse(slotDurationConfig?.ConfigValue, out var parsed)
                ? parsed
                : 60;

            var allSlots = new List<TimeOnly>();
            var currentTime = workerSchedule.StartTime;
            var endTime = workerSchedule.EndTime;

            while (currentTime < endTime)
            {
                allSlots.Add(currentTime);
                currentTime = currentTime.AddMinutes(slotDuration);
            }

            var bookedTimes = await context.Appointments
                .Where(a =>
                    a.WorkerId == workerId &&
                    a.AppointmentDate == date &&
                    a.Status != "cancelled")
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            var availableSlots = allSlots
                .Where(slot => !bookedTimes.Contains(slot))
                .ToList();

            var nowInTurkey = GetTurkeyTime();
            var todayInTurkey = DateOnly.FromDateTime(nowInTurkey);

            if (date == todayInTurkey)
            {
                var currentTimeInTurkey = TimeOnly.FromDateTime(nowInTurkey);

                int remainder = currentTimeInTurkey.Minute % slotDuration;
                if (remainder != 0)
                {
                    currentTimeInTurkey =
                        currentTimeInTurkey.AddMinutes(slotDuration - remainder);
                }

                availableSlots = availableSlots
                    .Where(slot => slot >= currentTimeInTurkey)
                    .ToList();

                logger.LogInformation(
                    "Filtering past slots. WorkerId={WorkerId}, Date={Date}, Now={Now}, AvailableCount={Count}",
                    workerId,
                    date,
                    currentTimeInTurkey,
                    availableSlots.Count);
            }

            return availableSlots
                .OrderBy(t => t)
                .ToList();
        }

        #endregion

        #region Appointment Methods

        public async Task<Appointment?> CreateAppointmentAsync(int userId, int workerId, DateOnly date, TimeOnly time,
            string? serviceType)
        {
            try
            {
                var nowInTurkey = GetTurkeyTime();
                var todayInTurkey = DateOnly.FromDateTime(nowInTurkey);
                var currentTimeInTurkey = TimeOnly.FromDateTime(nowInTurkey);

                if (date < todayInTurkey || (date == todayInTurkey && time <= currentTimeInTurkey))
                {
                    logger.LogWarning(
                        "Rejected past appointment. UserId={UserId} WorkerId={WorkerId} RequestedDate={Date} RequestedTime={Time} TurkeyNow={Now}",
                        userId, workerId, date, time, nowInTurkey);
                    return null;
                }

                var existingAppointment = await context.Appointments
                    .FirstOrDefaultAsync(a =>
                        a.WorkerId == workerId && a.AppointmentDate == date && a.AppointmentTime == time &&
                        a.Status != "cancelled");

                if (existingAppointment != null)
                {
                    logger.LogWarning("Time slot already booked for worker {WorkerId}: {Date} {Time}", workerId, date,
                        time);
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

                context.Appointments.Add(appointment);
                await context.SaveChangesAsync();

                logger.LogInformation(
                    "Created appointment for user {UserId} with worker {WorkerId} on {Date} at {Time}", userId,
                    workerId, date, time);
                return appointment;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create appointment for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> CancelAppointmentAsync(int userId, int appointmentId)
        {
            try
            {
                var appointment = await context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId);

                if (appointment == null)
                {
                    logger.LogWarning("Appointment not found: {AppointmentId} for user {UserId}", appointmentId,
                        userId);
                    return false;
                }

                appointment.Status = "cancelled";
                appointment.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                logger.LogInformation("Cancelled appointment {AppointmentId} for user {UserId}", appointmentId,
                    userId);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to cancel appointment {AppointmentId}", appointmentId);
                return false;
            }
        }

        public async Task<List<Appointment>> GetUserAppointmentsAsync(int userId)
        {
            return await context.Appointments
                .Include(a => a.Worker)
                .Where(a => a.UserId == userId && a.Status != "cancelled")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        #endregion
    }
}