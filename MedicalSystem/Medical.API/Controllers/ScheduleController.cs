using Microsoft.AspNetCore.Mvc;
using Medical.API.Data;
using Medical.API.Models;

namespace Medical.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly AppDbContext _context;

    public ScheduleController(AppDbContext context)
    {
        _context = context;
    }

    // CREATE SCHEDULE
    [HttpPost]
    public IActionResult CreateSchedule(Schedule schedule)
    {
        _context.Schedules.Add(schedule);
        _context.SaveChanges();

        GenerateTimeSlots(schedule);

        return Ok("Schedule created successfully");
    }

    // GET ALL SCHEDULES
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Schedules.ToList());
    }

    // TIME SLOT GENERATOR (CORE LOGIC)
    private void GenerateTimeSlots(Schedule schedule)
    {
        var start = schedule.Date.Add(schedule.StartTime);
        var end = schedule.Date.Add(schedule.EndTime);

        var slots = new List<TimeSlot>();

        while (start < end)
        {
            var slot = new TimeSlot
            {
                ScheduleId = schedule.ScheduleId,
                StartTime = start,
                EndTime = start.AddMinutes(schedule.SlotDuration),
                IsBooked = false
            };

            slots.Add(slot);

            start = start.AddMinutes(schedule.SlotDuration);
        }

        _context.TimeSlots.AddRange(slots);
        _context.SaveChanges();
    }
}