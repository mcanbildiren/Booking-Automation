using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HairdresserAdmin.Data;
using HairdresserAdmin.Models.ViewModels;
using System.Globalization;

namespace HairdresserAdmin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? date, int? workerId, int? month, int? year)
        {
            // Determine selected date
            DateOnly selectedDate;
            if (!string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var parsedDate))
            {
                selectedDate = parsedDate;
            }
            else
            {
                selectedDate = DateOnly.FromDateTime(DateTime.Today);
            }

            // Determine calendar month/year
            int calendarMonth = month ?? selectedDate.Month;
            int calendarYear = year ?? selectedDate.Year;

            // Build query for appointments
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Worker)
                .Where(a => a.AppointmentDate == selectedDate);

            // Filter by worker if specified
            if (workerId.HasValue && workerId.Value > 0)
            {
                query = query.Where(a => a.WorkerId == workerId.Value);
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentTime)
                .Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    Time = a.AppointmentTime.ToString("HH:mm"),
                    CustomerName = a.User.Name ?? "Misafir",
                    PhoneNumber = a.User.PhoneNumber,
                    Status = a.Status,
                    ServiceType = a.ServiceType,
                    Notes = a.Notes,
                    DurationMinutes = a.DurationMinutes,
                    WorkerId = a.WorkerId,
                    WorkerName = a.Worker != null ? a.Worker.Name : "Atanmamış"
                })
                .ToListAsync();

            // Get workers for filter dropdown
            var workers = await _context.Workers
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new WorkerFilterViewModel
                {
                    Id = w.Id,
                    Name = w.Name
                })
                .ToListAsync();

            // Build calendar data
            var calendarDays = BuildCalendarDays(calendarYear, calendarMonth, selectedDate, workerId);

            // Get month name in Turkish
            var culture = new CultureInfo("tr-TR");
            var monthName = culture.DateTimeFormat.GetMonthName(calendarMonth);

            var viewModel = new DashboardViewModel
            {
                SelectedDate = selectedDate,
                Appointments = appointments,
                TotalAppointments = appointments.Count,
                ConfirmedAppointments = appointments.Count(a => a.Status == "confirmed"),
                PendingAppointments = appointments.Count(a => a.Status == "pending"),
                CancelledAppointments = appointments.Count(a => a.Status == "cancelled"),
                CalendarDays = await calendarDays,
                CurrentMonth = calendarMonth,
                CurrentYear = calendarYear,
                MonthName = monthName,
                Workers = workers,
                SelectedWorkerId = workerId
            };

            return View(viewModel);
        }

        private async Task<List<CalendarDayViewModel>> BuildCalendarDays(int year, int month, DateOnly selectedDate, int? workerId)
        {
            var days = new List<CalendarDayViewModel>();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var firstDayOfMonth = new DateOnly(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Get appointment counts for the month
            var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek + 1); // Start from Monday
            var endDate = lastDayOfMonth.AddDays(7 - (int)lastDayOfMonth.DayOfWeek); // End at Sunday

            var query = _context.Appointments
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate && a.Status != "cancelled");

            if (workerId.HasValue && workerId.Value > 0)
            {
                query = query.Where(a => a.WorkerId == workerId.Value);
            }

            var appointmentCounts = await query
                .GroupBy(a => a.AppointmentDate)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            // Build 6 weeks of calendar
            var currentDate = startDate;
            if ((int)firstDayOfMonth.DayOfWeek == 0) // If month starts on Sunday
            {
                currentDate = firstDayOfMonth.AddDays(-6);
            }
            else
            {
                currentDate = firstDayOfMonth.AddDays(-((int)firstDayOfMonth.DayOfWeek - 1));
            }

            for (int i = 0; i < 42; i++) // 6 weeks
            {
                days.Add(new CalendarDayViewModel
                {
                    Date = currentDate,
                    Day = currentDate.Day,
                    IsCurrentMonth = currentDate.Month == month,
                    IsToday = currentDate == today,
                    IsSelected = currentDate == selectedDate,
                    AppointmentCount = appointmentCounts.GetValueOrDefault(currentDate, 0)
                });
                currentDate = currentDate.AddDays(1);
            }

            return days;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, int? workerId)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { date = appointment.AppointmentDate.ToString("yyyy-MM-dd"), workerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotes(int id, string notes, int? workerId)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Notes = notes;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { date = appointment.AppointmentDate.ToString("yyyy-MM-dd"), workerId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Worker)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // API endpoint for calendar events (optional, for AJAX)
        [HttpGet]
        public async Task<IActionResult> GetAppointments(string date, int? workerId)
        {
            if (!DateOnly.TryParse(date, out var selectedDate))
            {
                return BadRequest();
            }

            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Worker)
                .Where(a => a.AppointmentDate == selectedDate);

            if (workerId.HasValue && workerId.Value > 0)
            {
                query = query.Where(a => a.WorkerId == workerId.Value);
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentTime)
                .Select(a => new
                {
                    a.Id,
                    Time = a.AppointmentTime.ToString("HH:mm"),
                    CustomerName = a.User.Name ?? "Misafir",
                    a.User.PhoneNumber,
                    a.Status,
                    WorkerName = a.Worker != null ? a.Worker.Name : "Atanmamış"
                })
                .ToListAsync();

            return Json(appointments);
        }
    }
}

