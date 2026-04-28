using Microsoft.AspNetCore.Mvc;
using Medical.API.Data;
using Medical.API.DTOs;
using Medical.API.Models;
using Medical.API.Services;

namespace Medical.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext context, JwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterDto request)
    {
        if (_context.Users.Any(u => u.Email == request.Email))
            return BadRequest("Email already exists");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Patient"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("Registered successfully");
    }

 [HttpPost("login")]
public IActionResult Login([FromBody] LoginDto request)
{
    try
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized("Invalid email");

        if (string.IsNullOrEmpty(user.PasswordHash))
            return BadRequest("User password is not set properly");

        bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!valid)
            return Unauthorized("Invalid password");

        var token = _jwt.GenerateToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role
            }
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message);
    }
}
}