using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystemAPI.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }

        public string? Gender { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Disease { get; set; }

        // Navigation Property: one patient can have many appointments
        public ICollection<Appointment>? Appointments { get; set; }

        // Navigation Property: a patient may occupy one room (optional)
        public Room? Room { get; set; }
    }
}
