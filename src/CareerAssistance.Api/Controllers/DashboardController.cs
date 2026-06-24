using CareerAssistance.Application.DTOs.Dashboard;
using CareerAssistance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareerAssistance.Api.Controllers;

/// <summary>
/// Отримання агрегованої аналітики та метрик для головного екрана додатка.
/// </summary>
[Authorize]
[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IJobService _jobService;

    public DashboardController(IJobService jobService)
    {
        _jobService = jobService;
    }

    /// <summary>
    /// Отримати сумарну кількість вакансій на кожному етапі Kanban-дошки.
    /// </summary>
    /// <remarks>
    /// Метод автоматично ізолює дані поточного користувача. 
    /// Повертає лічильники для кожної колонки (Saved, Applied, Interview, Offer, Rejected) та сумарну активну кількість.
    /// </remarks>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Об'єкт DashboardAnalyticsResponse з лічильниками</returns>
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(DashboardAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAnalytics(CancellationToken cancellationToken)
    {
        var response = await _jobService.GetAnalyticsAsync(cancellationToken);
        return Ok(response);
    }
}