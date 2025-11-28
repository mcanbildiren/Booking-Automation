using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhatsAppBookingService.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("phone_number")]
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        [Column("name")]
        [StringLength(100)]
        public string? Name { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_contact")]
        public DateTime LastContact { get; set; } = DateTime.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}

