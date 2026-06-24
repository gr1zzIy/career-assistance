using CareerAssistance.Domain.Enums;

namespace CareerAssistance.Application.DTOs.Jobs;

public record JobResponse(
    Guid Id,
    string Title,
    string Company,
    string? Description,
    string? Url,
    string? Notes,
    string? SalaryRange,
    JobStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);