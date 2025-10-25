namespace HospitalManagementSystemAPI.DTOs
{
    public class BillingDTO
    {
        public int BillingId { get; set; }
        public int AppointmentId { get; set; }

        public decimal ConsultationFee { get; set; }
        public decimal MedicineCharges { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
    }
}
