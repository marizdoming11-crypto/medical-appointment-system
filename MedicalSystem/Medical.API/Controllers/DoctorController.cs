using Microsoft.AspNetCore.Mvc;
using Medical.API.Data;
using Medical.API.Models;

namespace Medical.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly AppDbContext _context;

    public DoctorController(AppDbContext context)
    {
        _context = context;
    }

    // ➕ Add Doctor
    [HttpPost]
    public IActionResult AddDoctor(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
        _context.SaveChanges();
        return Ok("Doctor added successfully");
    }

    // 📄 Get All Doctors
    [HttpGet]
    public IActionResult GetDoctors()
    {
        return Ok(_context.Doctors.ToList());
    }
}