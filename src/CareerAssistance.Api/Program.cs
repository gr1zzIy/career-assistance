using CareerAssistance.Application.Interfaces;
using CareerAssistance.Api.Services;
using CareerAssistance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Додаємо підтримку контролерів
builder.Services.AddControllers();

// 2. Вбудована генерація OpenAPI (документація API)
builder.Services.AddOpenApi();

// 3. Реєстрація нашого сервісу користувача (тимчасова заглушка до впровадження JWT)
builder.Services.AddScoped<ICurrentUserService, TestUserService>();

// 4. Підключення нашого шару інфраструктури (DbContext, PostgreSQL)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Налаштування HTTP-пайплайну запитів
if (app.Environment.IsDevelopment())
{
    // Дозволяємо генерацію JSON-файлу специфікації API в режимі розробки
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Обов'язково додаємо автентифікацію та авторизацію в пайплайн
// Навіть якщо зараз вони працюють у базовому режимі, вони необхідні для майбутнього JWT
app.UseAuthentication();
app.UseAuthorization();

// Мапимо наші майбутні контролери
app.MapControllers();

app.Run();