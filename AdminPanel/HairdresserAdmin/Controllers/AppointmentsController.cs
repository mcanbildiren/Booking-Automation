using System.Globalization;
using HairdresserAdmin.Data;
using HairdresserAdmin.Models;
using HairdresserAdmin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HairdresserAdmin.Controllers
{
    [Authorize]
    public class AppointmentsController(ApplicationDbContext context) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(string? date, int? workerId, string? status, string? search)
        {
            var selectedDate = ParseDateOrDefault(date, DateOnly.FromDateTime(DateTime.Today));

            var query = context.Appointments
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Worker)
                .Where(a => a.AppointmentDate == selectedDate);

            if (workerId.HasValue && workerId.Value > 0)
            {
                query = query.Where(a => a.WorkerId == workerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                var sLower = s.ToLower();
                query = query.Where(a =>
                    (a.User.Name != null && a.User.Name.ToLower().Contains(sLower)) ||
                    a.User.PhoneNumber.Contains(s) ||
                    (a.ServiceType != null && a.ServiceType.ToLower().Contains(sLower)));
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentTime)
                .Select(a => new AppointmentRowViewModel
                {
                    Id = a.Id,
                    Time = a.AppointmentTime.ToString("HH:mm"),
                    CustomerName = a.User.Name ?? "Misafir",
                    PhoneNumber = a.User.PhoneNumber,
                    WorkerName = a.Worker != null ? a.Worker.Name : "Atanmamış",
                    Status = a.Status,
                    StatusText = StatusText(a.Status),
                    StatusBadgeClass = StatusBadgeClass(a.Status),
                    DurationMinutes = a.DurationMinutes,
                    ServiceType = a.ServiceType
                })
                .ToListAsync();

            var workers = await context.Workers
                .AsNoTracking()
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new WorkerFilterViewModel { Id = w.Id, Name = w.Name })
                .ToListAsync();

            var vm = new AppointmentIndexViewModel
            {
                SelectedDate = selectedDate.ToString("yyyy-MM-dd"),
                SelectedWorkerId = workerId,
                SelectedStatus = status,
                Search = search,
                Workers = workers,
                Appointments = appointments
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? date, int? workerId, string? time)
        {
            var selectedDate = ParseDateOrDefault(date, DateOnly.FromDateTime(DateTime.Today));
            var selectedTime = ParseTimeOrDefault(time, new TimeOnly(9, 0));

            await PopulateWorkers();

            var vm = new AppointmentCreateViewModel
            {
                AppointmentDate = selectedDate.ToString("yyyy-MM-dd"),
                AppointmentTime = selectedTime.ToString("HH:mm"),
                WorkerId = workerId.GetValueOrDefault() > 0 ? workerId!.Value : 0
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            await PopulateWorkers();

            if (!TryParseDateTime(model.AppointmentDate, model.AppointmentTime, out var date, out var time))
            {
                ModelState.AddModelError(string.Empty, "Tarih veya saat formatı geçersiz.");
            }

            var normalizedPhone = NormalizePhone(model.PhoneNumber);
            if (string.IsNullOrWhiteSpace(normalizedPhone))
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "Telefon zorunludur.");
            }

            if (model.WorkerId <= 0)
            {
                ModelState.AddModelError(nameof(model.WorkerId), "Çalışan seçiniz.");
            }

            if (!IsValidStatus(model.Status))
            {
                ModelState.AddModelError(nameof(model.Status), "Geçersiz durum.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var workerExists = await context.Workers.AnyAsync(w => w.Id == model.WorkerId);
            if (!workerExists)
            {
                ModelState.AddModelError(nameof(model.WorkerId), "Çalışan bulunamadı.");
                return View(model);
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone);
            if (user == null)
            {
                user = new User
                {
                    PhoneNumber = normalizedPhone,
                    Name = string.IsNullOrWhiteSpace(model.CustomerName) ? null : model.CustomerName.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    LastContact = DateTime.UtcNow
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
            else if (!string.IsNullOrWhiteSpace(model.CustomerName) && string.IsNullOrWhiteSpace(user.Name))
            {
                user.Name = model.CustomerName.Trim();
                user.LastContact = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            var conflict = await HasExactSlotConflict(workerId: model.WorkerId, date: date, time: time, excludeAppointmentId: null);
            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "Bu çalışan için seçilen tarih/saatte zaten bir randevu var.");
                return View(model);
            }

            var appointment = new Appointment
            {
                UserId = user.Id,
                WorkerId = model.WorkerId,
                AppointmentDate = date,
                AppointmentTime = time,
                DurationMinutes = model.DurationMinutes,
                Status = model.Status,
                ServiceType = string.IsNullOrWhiteSpace(model.ServiceType) ? null : model.ServiceType.Trim(),
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { date = appointment.AppointmentDate.ToString("yyyy-MM-dd"), workerId = appointment.WorkerId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await context.Appointments
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Worker)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            await PopulateWorkers();

            var vm = new AppointmentEditViewModel
            {
                Id = appointment.Id,
                CustomerDisplay = $"{(appointment.User.Name ?? "Misafir")} (+{appointment.User.PhoneNumber})",
                WorkerId = appointment.WorkerId,
                AppointmentDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                AppointmentTime = appointment.AppointmentTime.ToString("HH:mm"),
                DurationMinutes = appointment.DurationMinutes,
                Status = appointment.Status,
                ServiceType = appointment.ServiceType,
                Notes = appointment.Notes
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppointmentEditViewModel model)
        {
            await PopulateWorkers();

            if (!TryParseDateTime(model.AppointmentDate, model.AppointmentTime, out var date, out var time))
            {
                ModelState.AddModelError(string.Empty, "Tarih veya saat formatı geçersiz.");
            }

            if (!IsValidStatus(model.Status))
            {
                ModelState.AddModelError(nameof(model.Status), "Geçersiz durum.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var appointment = await context.Appointments.FirstOrDefaultAsync(a => a.Id == model.Id);
            if (appointment == null) return NotFound();

            var conflict = await HasExactSlotConflict(workerId: model.WorkerId, date: date, time: time, excludeAppointmentId: model.Id);
            if (conflict)
            {
                ModelState.AddModelError(string.Empty, "Bu çalışan için seçilen tarih/saatte zaten bir randevu var.");
                return View(model);
            }

            appointment.WorkerId = model.WorkerId;
            appointment.AppointmentDate = date;
            appointment.AppointmentTime = time;
            appointment.DurationMinutes = model.DurationMinutes;
            appointment.Status = model.Status;
            appointment.ServiceType = string.IsNullOrWhiteSpace(model.ServiceType) ? null : model.ServiceType.Trim();
            appointment.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
            appointment.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { date = appointment.AppointmentDate.ToString("yyyy-MM-dd"), workerId = appointment.WorkerId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await context.Appointments
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Worker)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var vm = new AppointmentDeleteViewModel
            {
                Id = appointment.Id,
                Date = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                Time = appointment.AppointmentTime.ToString("HH:mm"),
                WorkerName = appointment.Worker?.Name ?? "Atanmamış",
                CustomerName = appointment.User.Name ?? "Misafir",
                PhoneNumber = appointment.User.PhoneNumber,
                StatusText = StatusText(appointment.Status),
                ServiceType = appointment.ServiceType
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null) return NotFound();

            var redirectDate = appointment.AppointmentDate.ToString("yyyy-MM-dd");
            var redirectWorkerId = appointment.WorkerId;

            context.Appointments.Remove(appointment);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { date = redirectDate, workerId = redirectWorkerId });
        }

        private async Task PopulateWorkers()
        {
            var workers = await context.Workers
                .AsNoTracking()
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .ToListAsync();

            ViewBag.Workers = workers;
        }

        private static bool TryParseDateTime(string date, string time, out DateOnly d, out TimeOnly t)
        {
            var dateOk = DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
            var timeOk = TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out t);
            return dateOk && timeOk;
        }

        private static DateOnly ParseDateOrDefault(string? date, DateOnly fallback)
        {
            if (!string.IsNullOrWhiteSpace(date) &&
                DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                return d;
            }
            return fallback;
        }

        private static TimeOnly ParseTimeOrDefault(string? time, TimeOnly fallback)
        {
            if (!string.IsNullOrWhiteSpace(time) &&
                TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
            {
                return t;
            }
            return fallback;
        }

        private static bool IsValidStatus(string status) =>
            status is "pending" or "confirmed" or "cancelled" or "completed";

        private static string StatusText(string status) => status switch
        {
            "confirmed" => "Onaylandı",
            "pending" => "Bekliyor",
            "cancelled" => "İptal",
            "completed" => "Tamamlandı",
            _ => status
        };

        private static string StatusBadgeClass(string status) => status switch
        {
            "confirmed" => "badge bg-success",
            "pending" => "badge bg-warning",
            "cancelled" => "badge bg-danger",
            "completed" => "badge bg-info",
            _ => "badge bg-secondary"
        };

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.Trim();
        }

        private async Task<bool> HasExactSlotConflict(int workerId, DateOnly date, TimeOnly time, int? excludeAppointmentId)
        {
            var q = context.Appointments
                .AsNoTracking()
                .Where(a => a.WorkerId == workerId &&
                            a.AppointmentDate == date &&
                            a.AppointmentTime == time &&
                            a.Status != "cancelled");

            if (excludeAppointmentId.HasValue)
            {
                q = q.Where(a => a.Id != excludeAppointmentId.Value);
            }

            return await q.AnyAsync();
        }
    }
}