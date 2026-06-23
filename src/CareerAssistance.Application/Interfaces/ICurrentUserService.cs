namespace CareerAssistance.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}