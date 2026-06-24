using CareerAssistance.Application.DTOs.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CareerAssistance.Application.Interfaces;
using CareerAssistance.Domain.Entities;
using CareerAssistance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace CareerAssistance.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;
    private readonly IConfiguration _configuration;
    
    public AuthService(
        ApplicationDbContext dbContext, 
        IOptions<JwtSettings> jwtSettings,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
        _configuration = configuration;
    }
    
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Перевіряємо унікальність Email
        var emailExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }
        
        // Хешуємо пароль за допомогою стабільного алгоритму BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User()
        {
            Email = request.Email,
            PasswordHash = passwordHash
        };
        
        _dbContext.Users.Add(user);
        
        // Генеруємо первинну пару токенів
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user.Id);
        
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new AuthResponse(accessToken, refreshToken.Token, refreshToken.ExpiresAt);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request, 
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        
        // Оновлюємо токени при кожному вході
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user.Id);
        
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken.Token, refreshToken.ExpiresAt);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default)
    {
        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        // Маркуємо старий токен як використаний (відкликаний) за принципом безпеки "Rotation"
        storedToken.IsRevoked = true;

        // Створюємо нову пару токенів
        var newAccessToken = GenerateAccessToken(storedToken.User);
        var newRefreshToken = GenerateRefreshToken(storedToken.UserId);

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(newAccessToken, newRefreshToken.Token, newRefreshToken.ExpiresAt);
    }

    public async Task<AuthResponse> LoginWithGoogleAsync(
        string idToken, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Валідуємо токен через бібліотеку Google
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                // Перевіряємо, що токен призначений саме для нашого додатка
                //todo поки без фронту то комент Audience = new[] { _configuration["Authentication:Google:ClientId"] }
            };

            // Якщо токен підроблений або прострочений - цей метод викине ексепшн
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            // Витягуємо email користувача, який Google вже залізно підтвердив
            var email = payload.Email;

            // Шукаємо користувача в нашій базі даних
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null)
            {
                // Якщо користувача немає - це його перший вхід (автоматична реєстрація!)
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    PasswordHash = "" // Для Google-юзерів пароль порожній, вони входять без нього
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // Користувач перевірений. Генеруємо для нього НАШІ власні токені
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user.Id);

            // Зберігаємо рефреш-токен у базу
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AuthResponse(accessToken, refreshToken.Token, refreshToken.ExpiresAt);
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedAccessException("Невалідний Google ID токен.");
        }
    }

    #region Helpers

    /// <summary>
    /// Генератор токена доступа
    /// </summary>
    /// <param name="user">Дані про юзера</param>
    /// <returns>Токен</returns>
    private string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescription);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Генерує рефреш токен
    /// </summary>
    /// <param name="userId">id юзера</param>
    /// <returns>сущность для рефреш токена юзера</returns>
    private RefreshToken GenerateRefreshToken(Guid userId)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return new RefreshToken()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(randomNumber),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            IsRevoked = false
        };
    }

    #endregion
}