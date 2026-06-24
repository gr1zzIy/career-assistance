using CareerAssistance.Application.DTOs.Auth;
using CareerAssistance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareerAssistance.Api.Controllers;

/// <summary>
/// Управління автентифікацією: реєстрація, авторизація та оновлення сесій (JWT).
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    /// <summary>
    /// Реєстрація нового користувача у системі.
    /// </summary>
    /// <remarks>
    /// Створює новий обліковий запис кандидата. Пароль обов'язково перевіряється на мінімальну довжину (від 8 символів).
    /// Після успішної реєстрації автоматично створюється унікальний Tenant ID для ізоляції майбутніх вакансій.
    /// </remarks>
    /// <param name="request">Дані для реєстрації (Email та Пароль)</param>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Повертає пару токенів (Access та Refresh) для миттєвого входу</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
    
    /// <summary>
    /// Авторизація користувача (Вхід у систему).
    /// </summary>
    /// <remarks>
    /// Перевіряє пару Email/Password. У разі успіху генерує JWT токен (Access Token), 
    /// який потрібно передавати у заголовку "Authorization: Bearer {token}" для доступу до вакансій.
    /// </remarks>
    /// <param name="request">Логін та пароль користувача</param>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Об'єкт з Access Token та Refresh Token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, 
        CancellationToken cancellationToken)
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

    /// <summary>
    /// Оновлення згасаючої сесії за допомогою Refresh Token.
    /// </summary>
    /// <remarks>
    /// Дозволяє фронтенду безшумно оновити Access Token без повторного введення пароля користувачем.
    /// Переданий Refresh Token перевіряється на термін дії та валідність у базі даних.
    /// </remarks>
    /// <param name="request">Об'єкт, що містить поточний Refresh Token</param>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Нова пара Access та Refresh токенів</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequestDto request, 
        CancellationToken cancellationToken)
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

/// <summary>
/// Об'єкт запиту для оновлення токенів авторизації.
/// </summary>
/// <param name="RefreshToken">Діючий токен оновлення сесії</param>
public record RefreshRequestDto(string RefreshToken);