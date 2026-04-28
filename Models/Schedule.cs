namespace Medical.API.Models;

public class Schedule
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public string TimeSlot { get; set; } = "";   // e.g. "09:00 AM"
    public string Day { get; set; } = "Monday";
}