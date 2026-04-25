using Microsoft.AspNetCore.Mvc;
using Medical.API.Data;
using Medical.API.Models;

namespace Medical.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentController(AppDbContext context)
    {
        _context = context;
    }

    // 📅 Book Appointment
    [HttpPost]
    public IActionResult Book(Appointment appointment)
    {
        // ❗ Check if doctor already has appointment at same time
        var conflict = _context.Appointments.Any(a =>
            a.DoctorId == appointment.DoctorId &&
            a.AppointmentDate == appointment.AppointmentDate);

        if (conflict)
        {
            return BadRequest("Doctor already has an appointment at this time.");
        }

        appointment.Status = "Pending";

        _context.Appointments.Add(appointment);
        _context.SaveChanges();

        return Ok("Appointment booked successfully");
    }

    // 📄 Get all appointments
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Appointments.ToList());
    }
}
