using System.Security.Claims;
using CareerAssistance.Application.Interfaces;

namespace CareerAssistance.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            // Шукаємо унікальний ідентифікатор користувача в Claims поточного JWT токена
            var nameIdentifier = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(nameIdentifier, out var parsedGuid))
            {
                return parsedGuid;
            }

            // Якщо користувач не авторизований або токен відсутній, повертаємо null.
            // Global Query Filter у DbContext автоматично заблокує доступ до даних.
            return null;
        }
    }
}