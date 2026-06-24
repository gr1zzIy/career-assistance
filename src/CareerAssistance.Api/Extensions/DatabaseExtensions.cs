using CareerAssistance.Application.Interfaces;
using CareerAssistance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CareerAssistance.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<IApplicationDbContext>();
            
            // Викликаємо наш ініціалізатор, який сам накотить міграції та закине демо-дані
            await DbInitializer.SeedDataAsync(context);
        }
        catch (Exception ex)
        {
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseExtensions");
            
            // Якщо база не піднялася при старті контейнера - краще відразу "впустити" додаток, 
            // щоб Docker зміг його перезапустити (Fail-Fast принцип)
            logger.LogError(ex, "Критична помилка під час автоматичного старту бази даних або сідингу.");
            throw; 
        }
    }
}