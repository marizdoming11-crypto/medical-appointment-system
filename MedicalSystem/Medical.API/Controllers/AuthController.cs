using Microsoft.AspNetCore.Mvc;
using Medical.API.Data;
using Medical.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Medical.API.DTOs;

namespace Medical.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto request)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.Email == request.Email && u.PasswordHash == request.Password);

        if (user == null)
            return Unauthorized("Invalid credentials");

        var token = GenerateToken(user);

        return Ok(new { token, user });
    }
  
  

    private string GenerateToken(User user)
    {
        // Read config values with GetValue and validate key presence
        var jwtKey = _config.GetValue<string>("Jwt:Key");
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("Configuration error: 'Jwt:Key' is missing or empty.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _config.GetValue<string>("Jwt:Issuer") ?? string.Empty;
        var audience = _config.GetValue<string>("Jwt:Audience") ?? string.Empty;

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto request)
    {
        var exists = _context.Users.Any(u => u.Email == request.Email);

        if (exists)
            return BadRequest("Email already exists");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = request.Password,
            Role = "Patient"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("User registered");
    }
}