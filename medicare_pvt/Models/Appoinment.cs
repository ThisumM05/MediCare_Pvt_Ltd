using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicare_pvt.Models
{
    [Table("appointments")]
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled, Rescheduled

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(1000)]
        public string? Prescription { get; set; }

        [StringLength(500)]
        public string? Diagnosis { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Fee { get; set; }

        [StringLength(20)]
        public string? PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;
    }
}
