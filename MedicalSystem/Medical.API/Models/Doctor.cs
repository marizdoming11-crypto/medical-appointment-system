namespace Medical.API.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public string? FullName { get; set; }
        public string? Specialty { get; set; }
        public string? Email { get; set; }
    }
}
