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

    // Create appointment (book)
    [HttpPost("book")]
    public IActionResult Book(Appointment appointment)
    {
        var slot = _context.TimeSlots
            .FirstOrDefault(s => s.TimeSlotId == appointment.TimeSlotId);

        if (slot == null)
            return BadRequest("Invalid slot");

        if (slot.IsBooked)
            return BadRequest("Slot already booked");

        slot.IsBooked = true;

        appointment.Status ??= "Pending";

        _context.Appointments.Add(appointment);
        _context.SaveChanges();

        return Ok("Appointment booked successfully");
    }

    // Cancel appointment (keeps record, frees slot)
    [HttpPatch("{id}/cancel")]
    public IActionResult Cancel(int id)
    {
        var appt = _context.Appointments.Find(id);

        if (appt == null)
            return NotFound();

        var slot = _context.TimeSlots.Find(appt.TimeSlotId);

        if (slot != null)
            slot.IsBooked = false;

        appt.Status = "Cancelled";

        _context.SaveChanges();

        return Ok("Appointment cancelled");
    }

    // Get all appointments
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Appointments.ToList());
    }

    // Get appointment by id
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var appt = _context.Appointments.Find(id);

        if (appt == null)
            return NotFound();

        return Ok(appt);
    }

    // Update appointment (reschedule)
    [HttpPut("{id}")]
    public IActionResult Update(int id, Appointment updated)
    {
        var appt = _context.Appointments.Find(id);

        if (appt == null)
            return NotFound();

        var oldSlot = _context.TimeSlots.Find(appt.TimeSlotId);
        var newSlot = _context.TimeSlots.Find(updated.TimeSlotId);

        if (newSlot == null)
            return BadRequest("Invalid new slot");

        if (newSlot.IsBooked)
            return BadRequest("New slot is already booked");

        if (oldSlot != null)
            oldSlot.IsBooked = false;

        newSlot.IsBooked = true;

        appt.TimeSlotId = updated.TimeSlotId;
        appt.DoctorId = updated.DoctorId;
        appt.UserId = updated.UserId;
        appt.Status = updated.Status ?? appt.Status;

        _context.SaveChanges();

        return Ok("Appointment updated");
    }

    // Delete appointment (removes record and frees slot)
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var appt = _context.Appointments.Find(id);

        if (appt == null)
            return NotFound();

        var slot = _context.TimeSlots.Find(appt.TimeSlotId);

        if (slot != null)
            slot.IsBooked = false;

        _context.Appointments.Remove(appt);
        _context.SaveChanges();

        return Ok("Appointment deleted");
    }

    // Get available slots for a doctor
    [HttpGet("available/{doctorId}")]
    public IActionResult GetAvailableSlots(int doctorId)
    {
        var slots = _context.TimeSlots
            .Where(s => !s.IsBooked &&
                        _context.Schedules.Any(sc =>
                            sc.ScheduleId == s.ScheduleId &&
                            sc.DoctorId == doctorId))
            .ToList();

        return Ok(slots);
    }
}