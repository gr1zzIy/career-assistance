using CareerAssistance.Application.Interfaces;
using CareerAssistance.Infrastructure.Identity;
using CareerAssistance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareerAssistance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Конфігурація JWT налаштувань
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b => 
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Реєстрація бізнес-сервісу автентифікації
        services.AddScoped<IAuthService, AuthService>();
        
        return services;
    }
}