using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhatsAppBookingService.Models
{
    [Table("worker_schedules")]
    public class WorkerSchedule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("worker_id")]
        public int WorkerId { get; set; }

        [Column("day_of_week")]
        [Required]
        public int DayOfWeek { get; set; }

        [Column("start_time")]
        [Required]
        public TimeOnly StartTime { get; set; }

        [Column("end_time")]
        [Required]
        public TimeOnly EndTime { get; set; }

        [Column("is_working")]
        public bool IsWorking { get; set; } = true;

        [ForeignKey("WorkerId")]
        public Worker Worker { get; set; } = null!;
    }
}

