using System.Reflection;
using CareerAssistance.Application.Interfaces;
using CareerAssistance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerAssistance.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public DbSet<User> Users => Set<User>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Автоматично застосовуємо всі Fluent API конфігурації з поточної збірки
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Глобальний фільтр ізоляції даних користувача.
        // Якщо користувач не авторизований (UserId == null), фільтр не поверне нічого.
        modelBuilder.Entity<Job>().HasQueryFilter(j => j.UserId == _currentUserService.UserId);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries())
        {
            // Автоматичне заповнення аудит-полів для сутності Job
            if (entry.Entity is Job job)
            {
                if (entry.State == EntityState.Added)
                {
                    job.CreatedAt = DateTime.UtcNow;
                    
                    // Якщо UserId ще не було присвоєно вручну, беремо його з контексту авторизації
                    if (job.UserId == Guid.Empty && currentUserId.HasValue)
                    {
                        job.UserId = currentUserId.Value;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    job.UpdatedAt = DateTime.UtcNow;
                }
            }
            
            // Автоматичне заповнення для сутності User та токенів під час створення
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is User user)
                {
                    user.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is RefreshToken token)
                {
                    token.CreatedAt = DateTime.UtcNow;
                }
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}