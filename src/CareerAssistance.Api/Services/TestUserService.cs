using CareerAssistance.Application.Interfaces;

namespace CareerAssistance.Api.Services;

public class TestUserService : ICurrentUserService 
{ 
    // Тимчасово повертаємо згенерований Guid для дизайну та перших тестів
    public Guid? UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");
}