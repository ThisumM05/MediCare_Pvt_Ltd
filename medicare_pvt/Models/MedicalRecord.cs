using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicare_pvt.Models
{
    public class MedicalRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Diagnosis { get; set; }

        [StringLength(2000)]
        public string? Prescription { get; set; }

        [StringLength(1000)]
        public string? LabResults { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(500)]
        public string? VitalSigns { get; set; }

        [StringLength(500)]
        public string? Symptoms { get; set; }

        [StringLength(500)]
        public string? TreatmentPlan { get; set; }

        public DateTime RecordDate { get; set; } = DateTime.Now;

        public DateTime? FollowUpDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; } = null!;

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; } = null!;
    }
}
