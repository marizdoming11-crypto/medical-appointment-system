using Microsoft.AspNetCore.Mvc;
using Medical.API.Data;
using Medical.API.Models;

namespace Medical.API.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("book")]
        public IActionResult Book(Appointment appointment)
        {
            appointment.Status = "pending";
            _context.Appointments.Add(appointment);
            _context.SaveChanges();
            return Ok("Appointment booked");
        }

        [HttpGet("all")]
        public IActionResult GetAll()
        {
            return Ok(_context.Appointments.ToList());
        }

        [HttpPut("approve/{id}")]
        public IActionResult Approve(int id)
        {
            var appt = _context.Appointments.Find(id);
            if (appt == null) return NotFound();

            appt.Status = "approved";
            _context.SaveChanges();

            return Ok("Approved");
        }
    }
}