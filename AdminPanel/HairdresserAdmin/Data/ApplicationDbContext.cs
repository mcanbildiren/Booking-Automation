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

            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("appointments");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.AppointmentDate, e.AppointmentTime });
                entity.Property(e => e.Status).HasDefaultValue("pending");
                entity.Property(e => e.DurationMinutes).HasDefaultValue(60);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

                // Configure relationship
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Appointments)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

