using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhatsAppBookingService.Models
{
    [Table("workers")]
    public class Worker
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Column("specialty")]
        [StringLength(100)]
        public string? Specialty { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<WorkerSchedule> Schedules { get; set; } = new List<WorkerSchedule>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}

