using Microsoft.EntityFrameworkCore;
using WhatsAppBookingService.Models;

namespace WhatsAppBookingService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<BusinessConfig> BusinessConfigs { get; set; } = null!;
        public DbSet<Worker> Workers { get; set; } = null!;
        public DbSet<WorkerSchedule> WorkerSchedules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            // Configure Worker
            modelBuilder.Entity<Worker>()
                .HasIndex(w => w.Name);

            // Configure WorkerSchedule
            modelBuilder.Entity<WorkerSchedule>()
                .HasOne(ws => ws.Worker)
                .WithMany(w => w.Schedules)
                .HasForeignKey(ws => ws.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkerSchedule>()
                .HasIndex(ws => new { ws.WorkerId, ws.DayOfWeek })
                .IsUnique();

            // Configure Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Worker)
                .WithMany(w => w.Appointments)
                .HasForeignKey(a => a.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: same worker can't have two appointments at same date/time
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.WorkerId, a.AppointmentDate, a.AppointmentTime })
                .IsUnique();

            // Configure BusinessConfig
            modelBuilder.Entity<BusinessConfig>()
                .HasIndex(bc => bc.ConfigKey)
                .IsUnique();
        }
    }
}

