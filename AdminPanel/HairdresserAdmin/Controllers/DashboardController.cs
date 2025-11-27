using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HairdresserAdmin.Data;
using HairdresserAdmin.Models.ViewModels;

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

        public async Task<IActionResult> Index(string? date)
        {
            DateOnly selectedDate;
            if (!string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var parsedDate))
            {
                selectedDate = parsedDate;
            }
            else
            {
                selectedDate = DateOnly.FromDateTime(DateTime.Today);
            }

            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Where(a => a.AppointmentDate == selectedDate)
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
                    DurationMinutes = a.DurationMinutes
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                SelectedDate = selectedDate,
                Appointments = appointments,
                TotalAppointments = appointments.Count,
                ConfirmedAppointments = appointments.Count(a => a.Status == "confirmed"),
                PendingAppointments = appointments.Count(a => a.Status == "pending"),
                CancelledAppointments = appointments.Count(a => a.Status == "cancelled")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { date = appointment.AppointmentDate.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotes(int id, string notes)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Notes = notes;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { date = appointment.AppointmentDate.ToString("yyyy-MM-dd") });
        }

        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }
    }
}

