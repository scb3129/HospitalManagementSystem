using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystemAPI.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Specialization { get; set; }

        public string Phone { get; set; }

        public int Experience { get; set; }

        // Make navigation property nullable
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
