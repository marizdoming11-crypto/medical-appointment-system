using Microsoft.AspNetCore.Mvc;
using Medical.API.Data;
using Medical.API.Models;

namespace Medical.API.Controllers;

[ApiController]
[Route("api/schedule")]
public class ScheduleController : ControllerBase
{
    private readonly AppDbContext _context;

    public ScheduleController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{doctorId}")]
    public IActionResult Get(int doctorId)
    {
        var schedules = _context.Schedules
            .Where(s => s.DoctorId == doctorId)
            .ToList();

        return Ok(schedules);
    }
}