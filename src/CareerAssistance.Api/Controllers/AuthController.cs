using CareerAssistance.Application.DTOs.Auth;
using CareerAssistance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareerAssistance.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Валідація мінімальної довжини пароля на рівні API контролера для надійності
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            {
                return BadRequest("Password must be at least 8 characters long.");
            }

            var response = await _authService.RegisterAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            // Якщо email вже зайнятий, повертаємо статус 409 Conflict
            return Conflict(ex.Message);
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Якщо пароль або email невірні, повертаємо 401 Unauthorized
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}

public record RefreshRequestDto(string RefreshToken);