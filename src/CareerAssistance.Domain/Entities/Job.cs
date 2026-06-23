using CareerAssistance.Domain.Enums;

namespace CareerAssistance.Domain.Entities;

public class Job
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } 

    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public string? SalaryRange { get; set; }

    public JobStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}