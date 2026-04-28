using Microsoft.AspNetCore.Mvc;
using Medical.API.Data;
using Medical.API.Models;

namespace Medical.API.Controllers;

[ApiController]
[Route("api/doctor")]
public class DoctorController : ControllerBase
{
    private readonly AppDbContext _context;

    public DoctorController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetDoctors()
    {
        return Ok(_context.Doctors.ToList());
    }

    [HttpPost]
    public IActionResult AddDoctor(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
        _context.SaveChanges();
        return Ok(doctor);
    }
}