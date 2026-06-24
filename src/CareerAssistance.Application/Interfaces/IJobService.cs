using CareerAssistance.Application.DTOs.Dashboard;
using CareerAssistance.Application.DTOs.Jobs;
using CareerAssistance.Domain.Enums;

namespace CareerAssistance.Application.Interfaces;

public interface IJobService
{
    Task<JobResponse> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobResponse>> GetAllAsync(JobStatus? status = null, CancellationToken cancellationToken = default);
    Task<JobResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobResponse> UpdateAsync(Guid id, CreateJobRequest request, CancellationToken cancellationToken = default);
    Task<JobResponse> UpdateStatusAsync(Guid id, JobStatus newStatus, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DashboardAnalyticsResponse> GetAnalyticsAsync(CancellationToken cancellationToken = default);
}