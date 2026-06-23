using System.Text;
using CareerAssistance.Application.Interfaces;
using CareerAssistance.Api.Services;
using CareerAssistance.Infrastructure;
using CareerAssistance.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Додаємо HttpContextAccessor, щоб CurrentUserService міг читати сесію користувача
builder.Services.AddHttpContextAccessor();

// Реєструємо сервіс користувача
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Зчитуємо конфігурацію JWT для налаштування системи валідації
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JwtSettings are not configured in appsettings.json");

// Налаштовуємо сервіси автентифікації додатка (JWT Bearer)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero // Токен прострочується строго в секунду ліміту
    };
});

// Підключаємо інфраструктуру (PostgreSQL, DbContext, AuthService)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Порядок критично важливий: спочатку система розпізнає ХТО користувач, а потім перевіряє ЩО йому дозволено
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();