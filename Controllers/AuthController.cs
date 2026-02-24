using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementService.Services;
using OrderManagementService.DTOs;
using OrderManagementService.Data;

namespace OrderManagementService.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly AppDbContext _context;

    public AuthController(JwtService jwtService, AppDbContext context)
    {
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request.Username == "admin" && request.Password == "password")
        {
            var accessToken = _jwtService.GenerateToken("1", "Admin");
            var refreshToken = _jwtService.GenerateRefreshToken();

            await SaveRefreshToken("1", "Admin", refreshToken);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        if (request.Username == "user" && request.Password == "password")
        {
            var accessToken = _jwtService.GenerateToken("2", "User");
            var refreshToken = _jwtService.GenerateRefreshToken();

            await SaveRefreshToken("2", "User", refreshToken);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        return Unauthorized("Invalid credentials");
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
            return Unauthorized("Invalid refresh token");

        var newAccessToken = _jwtService.GenerateToken(
            storedToken.UserId,
            storedToken.Role
        );

        return Ok(new
        {
            AccessToken = newAccessToken
        });
    }

    private async Task SaveRefreshToken(string userId, string role, string refreshToken)
    {
        var token = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            Role = role,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }
}