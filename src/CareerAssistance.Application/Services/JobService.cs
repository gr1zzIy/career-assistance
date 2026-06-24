using CareerAssistance.Application.DTOs.Jobs;
using CareerAssistance.Application.Interfaces;
using CareerAssistance.Domain.Entities;
using CareerAssistance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CareerAssistance.Application.Services;

public class JobService : IJobService
{
    private readonly IApplicationDbContext _dbContext;
    
    public JobService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<JobResponse> CreateAsync(
        CreateJobRequest request, 
        CancellationToken cancellationToken = default)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Company = request.Company,
            Description = request.Description,
            Url = request.Url,
            Notes = request.Notes,
            SalaryRange = request.SalaryRange,
            Status = JobStatus.Saved // Бо це дефолт при створенні
        };
        
        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MapToResponse(job);
    }

    public async Task<IEnumerable<JobResponse>> GetAllAsync(
        JobStatus? status = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Jobs.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }
        
        var jobs = await query.ToListAsync(cancellationToken);
        
        return jobs.Select(MapToResponse);
    }

    public async Task<JobResponse?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken: cancellationToken);
        
        return job == null ? null : MapToResponse(job);
    }

    public async Task<JobResponse> UpdateAsync(
        Guid id, 
        CreateJobRequest request, 
        CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.Jobs
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken: cancellationToken);
        
        if (job == null)
        {
            throw new KeyNotFoundException($"Job entry with ID {id} not found.");
        }

        job.Title = request.Title;
        job.Company = request.Company;
        job.Description = request.Description;
        job.Url = request.Url;
        job.Notes = request.Notes;
        job.SalaryRange = request.SalaryRange;

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MapToResponse(job);
    }

    public async Task<JobResponse> UpdateStatusAsync(
        Guid id, 
        JobStatus newStatus, 
        CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.Jobs
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken: cancellationToken);
        
        if (job == null)
        {
            throw new KeyNotFoundException($"Job entry with ID {id} not found.");
        }
        
        job.Status = newStatus;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MapToResponse(job);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.Jobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
        
        if (job != null)
        {
            _dbContext.Jobs.Remove(job);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    #region Mapper

    private static JobResponse MapToResponse(Job job)
    {
        return new JobResponse(
            job.Id,
            job.Title,
            job.Company,
            job.Description,
            job.Url,
            job.Notes,
            job.SalaryRange,
            job.Status,
            job.CreatedAt,
            job.UpdatedAt);
    }

    #endregion
}