namespace Medical.API.Models
{
    public class TimeSlot
    {
        public int TimeSlotId { get; set; }

        public int ScheduleId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsBooked { get; set; } = false;
    }
}