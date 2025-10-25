namespace HospitalManagementSystemAPI.DTOs
{
    public class AppointmentDTO
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }

        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;

        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public int? BillingId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? BillingStatus { get; set; }
    }
}
