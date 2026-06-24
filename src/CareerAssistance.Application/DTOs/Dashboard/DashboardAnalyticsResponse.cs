namespace CareerAssistance.Application.DTOs.Dashboard;

/// <summary>
/// Об'єкт відповіді з агрегованою аналітикою для головного екрана та Kanban-дошки.
/// </summary>
public record DashboardAnalyticsResponse(
    int SavedCount,
    int AppliedCount,
    int InterviewCount,
    int OfferCount,
    int RejectedCount,
    int TotalActiveCount);