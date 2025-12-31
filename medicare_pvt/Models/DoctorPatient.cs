using System;
using System.ComponentModel.DataAnnotations;

namespace medicare_pvt.Models
{
    public class DoctorPatient
    {
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
    }
}
