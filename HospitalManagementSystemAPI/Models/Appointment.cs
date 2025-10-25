using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystemAPI.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }

        // Foreign Keys
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        // Navigation Properties
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Billing? Billing { get; set; }
    }
}
