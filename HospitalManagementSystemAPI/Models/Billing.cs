using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystemAPI.Models
{
    public class Billing
    {
        [Key]
        public int BillingId { get; set; }
        public int AppointmentId { get; set; }

        public decimal ConsultationFee { get; set; }
        public decimal MedicineCharges { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = string.Empty;

        // Navigation Property
        public Appointment? Appointment { get; set; }
    }
}
