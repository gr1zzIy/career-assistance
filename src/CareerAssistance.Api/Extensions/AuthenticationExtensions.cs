using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CareerAssistance.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Зчитуємо параметри безпеки напряму з конфігурації як рядки
        var secret = configuration["JwtSettings:Secret"] 
                     ?? throw new InvalidOperationException("JwtSettings:Secret is not configured in appsettings.json");
        
        if (Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException("JwtSettings:Secret must be at least 32 bytes (256 bits) long to comply with HMAC-SHA256 requirements.");
        }
        
        var issuer = configuration["JwtSettings:Issuer"] 
                     ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured in appsettings.json");
        
        var audience = configuration["JwtSettings:Audience"] 
                       ?? throw new InvalidOperationException("JwtSettings:Audience is not configured in appsettings.json");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}