namespace CareerAssistance.Application.DTOs.Jobs;

public record CreateJobRequest(
    string Title,
    string Company,
    string? Description,
    string? Url,
    string? Notes,
    string? SalaryRange);