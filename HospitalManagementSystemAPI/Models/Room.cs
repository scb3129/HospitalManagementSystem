using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystemAPI.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        // Changed to string to match frontend and DB
        public string BedNumber { get; set; } = string.Empty;

        public bool IsOccupied { get; set; }

        // Foreign key (nullable)
        public int? PatientId { get; set; }

        // Navigation property
        public Patient? Patient { get; set; }
    }
}
