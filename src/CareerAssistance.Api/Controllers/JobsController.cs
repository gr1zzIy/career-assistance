using CareerAssistance.Application.DTOs.Jobs;
using CareerAssistance.Application.Interfaces;
using CareerAssistance.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerAssistance.Api.Controllers;

/// <summary>
/// Управління вакансіями та Kanban-пайплайном користувача.
/// </summary>
[Authorize]
[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    
    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    /// <summary>
    /// Створення нової картки вакансії.
    /// </summary>
    /// <remarks>
    /// Приймає деталі вакансії. Нова вакансія автоматично створюється зі статусом "Saved" (1).
    /// Ідентифікатор користувача (UserId) та дата створення присвоюються автоматично на рівні бази даних.
    /// </remarks>
    /// <param name="request">Дані для створення вакансії</param>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Повертає об'єкт створеної вакансії з присвоєним ідентифікатором Guid</returns>
    [HttpPost]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _jobService.CreateAsync(request, cancellationToken);
        
        // 201 повертаємо
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
    
    /// <summary>
    /// Отримання списку вакансій поточного користувача.
    /// </summary>
    /// <remarks>
    /// Повертає лише вакансії, які належать авторизованому користувачу. 
    /// Доступна необов'язкова фільтрація за колонкою Kanban-дошки.
    /// </remarks>
    /// <param name="status">Числовий код статусу: 1 = Saved, 2 = Applied, 3 = Interview, 4 = Offer, 5 = Rejected</param>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Список об'єктів вакансій</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JobResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] JobStatus? status, CancellationToken cancellationToken)
    {
        // Отримання всіх вакансій поточного користувача. Можлива фільтрація за статусом: ?status=2
        var response = await _jobService.GetAllAsync(status, cancellationToken);
        return Ok(response);
    }
    
    /// <summary>
    /// Отримання детальної інформації про конкретну вакансію.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор вакансії (Guid)</param>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Об'єкт вакансії, якщо вона знайдена та належить поточному користувачу</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken)
    {
        var response = await _jobService.GetByIdAsync(id, cancellationToken);
        
        if (response == null)
        {
            return NotFound($"Job entry with ID {id} not found.");
        }

        return Ok(response);
    }

    /// <summary>
    /// Повне редагування текстових полів вакансії.
    /// </summary>
    /// <remarks>
    /// Дозволяє змінити назву, компанію, опис, посилання, нотатки та зарплатну вилку. 
    /// Статус вакансії цим запитом не змінюється.
    /// </remarks>
    /// <param name="id">Ідентифікатор вакансії, яку потрібно оновити</param>
    /// <param name="request">Нові текстові дані вакансії</param>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Оновлений об'єкт вакансії</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id, 
        [FromBody] CreateJobRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _jobService.UpdateAsync(id, request, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Швидка зміна статусу вакансії (переміщення по Kanban-дошці).
    /// </summary>
    /// <remarks>
    /// Використовується фронтендом під час перетягування картки між колонками. 
    /// Автоматично оновлює поле дати модифікації (UpdatedAt).
    /// </remarks>
    /// <param name="id">Ідентифікатор вакансії</param>
    /// <param name="request">Новий об'єкт статусу</param>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Об'єкт вакансії з новим статусом</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(JobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] Guid id, 
        [FromBody] UpdateJobStatusRequestDto request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _jobService.UpdateStatusAsync(id, request.Status, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Видалення картки вакансії із системи.
    /// </summary>
    /// <remarks>
    /// Повністю видаляє запис із бази даних. Якщо вакансія не існує або належить іншому користувачу, 
    /// запит виконається без помилки, забезпечуючи ідемпотентність операції.
    /// </remarks>
    /// <param name="id">Ідентифікатор вакансії для видалення</param>
    /// <param name="cancellationToken">Токен скасування операції</param>
    /// <returns>Порожня відповідь зі статус-кодом 204 No Content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken)
    {
        await _jobService.DeleteAsync(id, cancellationToken);
        return NoContent(); // 204 для успішного видалення
    }
}

/// <summary>
/// Об'єкт запиту для швидкої зміни статусу вакансії.
/// </summary>
/// <param name="Status">Новий статус вакансії у системі</param>
public record UpdateJobStatusRequestDto(JobStatus Status);