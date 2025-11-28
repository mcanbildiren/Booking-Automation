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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            // Configure Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.AppointmentDate, a.AppointmentTime })
                .IsUnique();

            // Configure BusinessConfig
            modelBuilder.Entity<BusinessConfig>()
                .HasIndex(bc => bc.ConfigKey)
                .IsUnique();
        }
    }
}

