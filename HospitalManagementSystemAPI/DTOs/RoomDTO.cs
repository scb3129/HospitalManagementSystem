namespace HospitalManagementSystemAPI.DTOs
{
    public class RoomDTO
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string BedNumber { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }

        public int? PatientId { get; set; }
        public string? PatientName { get; set; }
    }
}
