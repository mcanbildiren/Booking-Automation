using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HairdresserAdmin.Data;
using HairdresserAdmin.Models;
using HairdresserAdmin.Models.ViewModels;

namespace HairdresserAdmin.Controllers
{
    [Authorize]
    public class WorkersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            var workers = await _context.Workers
                .Include(w => w.Schedules)
                .Include(w => w.Appointments)
                .OrderBy(w => w.Name)
                .Select(w => new WorkerListViewModel
                {
                    Id = w.Id,
                    Name = w.Name,
                    Specialty = w.Specialty,
                    IsActive = w.IsActive,
                    TotalAppointments = w.Appointments.Count(a => a.Status != "cancelled"),
                    TodayAppointments = w.Appointments.Count(a => a.AppointmentDate == today && a.Status != "cancelled"),
                    WorkingDays = string.Join(", ", w.Schedules
                        .Where(s => s.IsWorking)
                        .OrderBy(s => s.DayOfWeek)
                        .Select(s => GetShortDayName(s.DayOfWeek)))
                })
                .ToListAsync();

            return View(workers);
        }

        public IActionResult Create()
        {
            var viewModel = new WorkerViewModel
            {
                Schedules = GetDefaultSchedules()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkerViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var worker = new Worker
                {
                    Name = viewModel.Name,
                    Specialty = viewModel.Specialty,
                    IsActive = viewModel.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Workers.Add(worker);
                await _context.SaveChangesAsync();

                foreach (var schedule in viewModel.Schedules.Where(s => s.IsWorking))
                {
                    var workerSchedule = new WorkerSchedule
                    {
                        WorkerId = worker.Id,
                        DayOfWeek = schedule.DayOfWeek,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        IsWorking = true
                    };
                    _context.WorkerSchedules.Add(workerSchedule);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"{worker.Name} başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            viewModel.Schedules = GetDefaultSchedules();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worker = await _context.Workers
                .Include(w => w.Schedules)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (worker == null)
            {
                return NotFound();
            }

            var viewModel = new WorkerViewModel
            {
                Id = worker.Id,
                Name = worker.Name,
                Specialty = worker.Specialty,
                IsActive = worker.IsActive,
                Schedules = GetSchedulesForWorker(worker)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkerViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var worker = await _context.Workers
                    .Include(w => w.Schedules)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (worker == null)
                {
                    return NotFound();
                }

                worker.Name = viewModel.Name;
                worker.Specialty = viewModel.Specialty;
                worker.IsActive = viewModel.IsActive;

                _context.WorkerSchedules.RemoveRange(worker.Schedules);

                foreach (var schedule in viewModel.Schedules.Where(s => s.IsWorking))
                {
                    var workerSchedule = new WorkerSchedule
                    {
                        WorkerId = worker.Id,
                        DayOfWeek = schedule.DayOfWeek,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        IsWorking = true
                    };
                    _context.WorkerSchedules.Add(workerSchedule);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"{worker.Name} başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worker = await _context.Workers
                .Include(w => w.Appointments)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (worker == null)
            {
                return NotFound();
            }

            return View(worker);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker != null)
            {
                var name = worker.Name;
                _context.Workers.Remove(worker);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"{name} başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var worker = await _context.Workers.FindAsync(id);
            if (worker == null)
            {
                return NotFound();
            }

            worker.IsActive = !worker.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{worker.Name} durumu güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        private List<WorkerScheduleViewModel> GetDefaultSchedules()
        {
            var schedules = new List<WorkerScheduleViewModel>();
            var dayNames = new[] { "Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi" };

            for (int i = 0; i < 7; i++)
            {
                schedules.Add(new WorkerScheduleViewModel
                {
                    DayOfWeek = i,
                    DayName = dayNames[i],
                    IsWorking = i >= 1 && i <= 6,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(18, 0)
                });
            }

            return schedules;
        }

        private List<WorkerScheduleViewModel> GetSchedulesForWorker(Worker worker)
        {
            var schedules = new List<WorkerScheduleViewModel>();
            var dayNames = new[] { "Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi" };

            for (int i = 0; i < 7; i++)
            {
                var existingSchedule = worker.Schedules.FirstOrDefault(s => s.DayOfWeek == i);
                schedules.Add(new WorkerScheduleViewModel
                {
                    Id = existingSchedule?.Id ?? 0,
                    DayOfWeek = i,
                    DayName = dayNames[i],
                    IsWorking = existingSchedule?.IsWorking ?? false,
                    StartTime = existingSchedule?.StartTime ?? new TimeOnly(9, 0),
                    EndTime = existingSchedule?.EndTime ?? new TimeOnly(18, 0)
                });
            }

            return schedules;
        }

        private static string GetShortDayName(int dayOfWeek) => dayOfWeek switch
        {
            0 => "Paz",
            1 => "Pzt",
            2 => "Sal",
            3 => "Çar",
            4 => "Per",
            5 => "Cum",
            6 => "Cmt",
            _ => ""
        };
    }
}

