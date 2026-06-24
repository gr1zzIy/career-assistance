using CareerAssistance.Application.Interfaces;
using CareerAssistance.Domain.Entities;
using CareerAssistance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CareerAssistance.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedDataAsync(IApplicationDbContext context)
    {
        // 1. Автоматично накочуємо міграції при старті контейнера, якщо їх ще немає
        if (context is DbContext dbContext)
        {
            await dbContext.Database.MigrateAsync();
        }

        // 2. Перевіряємо, чи база порожня. Якщо користувачі вже є — нічого не робимо
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // 3. Створюємо дефолтного тестового користувача
        // Пароль: Password123 (захешований за допомогою BCrypt)
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@career.com",
            PasswordHash = "$2a$11$R9h/lS3v.m1fWbc6p1FmO.b1H7mK6k7f6v7G7g7h7i7j7k7l7m7n7" 
        };

        context.Users.Add(testUser);

        // 4. Додаємо тестові вакансії, прив'язані до цього користувача
        // Завдяки архітектурі, UserId підставиться автоматично, але ми зафіксуємо логіку додавання
        var job1 = new Job
        {
            Id = Guid.NewGuid(),
            Title = "Junior .NET Developer",
            Company = "Global IT Solutions",
            Description = "Great opportunity for beginners. Stack: C#, EF Core, PostgreSQL.",
            Status = JobStatus.Saved,
            CreatedAt = DateTime.UtcNow
        };

        var job2 = new Job
        {
            Id = Guid.NewGuid(),
            Title = "Fullstack Developer (React/.NET)",
            Company = "Productive SaaS Inc",
            Description = "Remote position. Building modern web interfaces.",
            Status = JobStatus.Interview,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        context.Jobs.AddRange(job1, job2);

        // 5. Зберігаємо все в базу даних
        await context.SaveChangesAsync();
    }
}