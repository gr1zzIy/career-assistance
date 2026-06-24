using CareerAssistance.Application.Interfaces;
using CareerAssistance.Api.Services;
using CareerAssistance.Api.Extensions;
using CareerAssistance.Application.Services;
using CareerAssistance.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Реєстрація системних компонентів веб-рівня
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJobService, JobService>();

builder.Services.AddOpenApi();

// Підключення ізольованої автентифікації та інфраструктури
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Налаштування Пайплайну обробки HTTP-запитів (Middleware)
if (app.Environment.IsDevelopment())
{
    // Генерує чистий JSON за адресою /openapi/v1.json
    app.MapOpenApi();
    
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();