using Microsoft.AspNetCore.Mvc;
using Medical.API.Data;
using Medical.API.Models;

namespace Medical.API.Controllers;

[ApiController]
[Route("api/appointment")]
public class AppointmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult Book(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        _context.SaveChanges();
        return Ok(appointment);
    }

    [HttpGet("user/{id}")]
    public IActionResult GetByUser(int id)
    {
        return Ok(_context.Appointments.Where(a => a.UserId == id).ToList());
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Appointment updated)
    {
        var appt = _context.Appointments.Find(id);
        if (appt == null) return NotFound();

        appt.Status = updated.Status;
        appt.AppointmentDate = updated.AppointmentDate;

        _context.SaveChanges();
        return Ok(appt);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var appt = _context.Appointments.Find(id);
        if (appt == null) return NotFound();

        _context.Appointments.Remove(appt);
        _context.SaveChanges();

        return Ok("Deleted");
    }
}