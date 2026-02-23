using Microsoft.AspNetCore.Mvc;
using OrderManagementService.Services;
using OrderManagementService.DTOs;

namespace OrderManagementService.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Username == "admin" && request.Password == "password")
        {
            var token = _jwtService.GenerateToken("1", "Admin");
            return Ok(new { Token = token });
        }

        return Unauthorized("Invalid credentials");
    }
}