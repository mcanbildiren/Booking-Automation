using Microsoft.EntityFrameworkCore;
using HairdresserAdmin.Models;

namespace HairdresserAdmin.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<WorkerSchedule> WorkerSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.LastContact).HasDefaultValueSql("NOW()");
            });

            // Configure Worker entity
            modelBuilder.Entity<Worker>(entity =>
            {
                entity.ToTable("workers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            });

            // Configure WorkerSchedule entity
            modelBuilder.Entity<WorkerSchedule>(entity =>
            {
                entity.ToTable("worker_schedules");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.WorkerId, e.DayOfWeek }).IsUnique();
                entity.Property(e => e.IsWorking).HasDefaultValue(true);

                entity.HasOne(ws => ws.Worker)
                    .WithMany(w => w.Schedules)
                    .HasForeignKey(ws => ws.WorkerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("appointments");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.WorkerId, e.AppointmentDate, e.AppointmentTime });
                entity.Property(e => e.Status).HasDefaultValue("pending");
                entity.Property(e => e.DurationMinutes).HasDefaultValue(60);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

                // Configure relationships
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Appointments)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Worker)
                    .WithMany(w => w.Appointments)
                    .HasForeignKey(a => a.WorkerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

