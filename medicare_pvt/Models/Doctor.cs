using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicare_pvt.Models
{
    [Table("doctors")]
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [StringLength(100)]
        public required string Specialty { get; set; }

        [StringLength(500)]
        public string? Qualifications { get; set; }

        [StringLength(10)]
        public string? LicenseNumber { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ConsultationFee { get; set; }

        [StringLength(500)]
        public string? Availability { get; set; } // JSON or text format

        [StringLength(1000)]
        public string? Bio { get; set; }

        [StringLength(255)]
        public string? ProfileImage { get; set; }

        public int ExperienceYears { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<DoctorPatient> DoctorPatients { get; set; } = new List<DoctorPatient>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
