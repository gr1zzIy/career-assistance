using CareerAssistance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerAssistance.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Job> Jobs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}