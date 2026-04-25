namespace Medical.API.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        public int DoctorId { get; set; }

        public string? PatientName { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string? Status { get; set; } // Pending, Confirmed, Cancelled
    }
}