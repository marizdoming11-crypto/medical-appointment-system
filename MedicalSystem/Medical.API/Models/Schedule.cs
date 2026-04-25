namespace Medical.API.Models
{
    public class Schedule
    {
        public int ScheduleId { get; set; }

        public int DoctorId { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int SlotDuration { get; set; } // in minutes
    }
}