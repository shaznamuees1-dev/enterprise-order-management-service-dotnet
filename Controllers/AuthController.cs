using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementService.Services;
using OrderManagementService.DTOs;
using OrderManagementService.Data;
using OrderManagementService.Domain.Entities;

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
            return await GenerateLoginResponse("1", "Admin");
        }

        if (request.Username == "user" && request.Password == "password")
        {
            return await GenerateLoginResponse("2", "User");
        }

        return Unauthorized(new ErrorResponse
        {
            Message = "Invalid credentials",
            StatusCode = 401
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
        {
            return Unauthorized(new ErrorResponse
            {
                Message = "Invalid refresh token",
                StatusCode = 401
            });
        }

        var newAccessToken = _jwtService.GenerateToken(
            storedToken.UserId,
            storedToken.Role
        );

        return Ok(new BaseResponse<object>
        {
            Success = true,
            Message = "Token refreshed successfully.",
            Data = new
            {
                AccessToken = newAccessToken
            }
        });
    }

    private async Task<IActionResult> GenerateLoginResponse(string userId, string role)
    {
        var accessToken = _jwtService.GenerateToken(userId, role);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await SaveRefreshToken(userId, role, refreshToken);

        return Ok(new BaseResponse<object>
        {
            Success = true,
            Message = "Login successful.",
            Data = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }
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