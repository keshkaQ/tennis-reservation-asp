using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;
using TennisReservation.Infrastructure.Postgres.Extensions;
using TennisReservation.Infrastructure.Postgres.Seeding;
using TennisReservation.Infrastructure.Postgres.Services;
using TennisReservation.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Настройка JWT
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));

// Настройка логгера
builder.Host.UseSerilog();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Information)
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
    )
    .CreateLogger();

// Регистрация сервисов приложения (сокращенно!)
builder.Services.AddApplicationServices(builder.Configuration);

// Добавление контроллеров и Razor Pages
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tennis Reservation API",
        Version = "v1",
        Description = "API для управления теннисным клубом"
    });
});

// Аутентификация
builder.Services.AddApiAuthentification(
    Options.Create(builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()));

var app = builder.Build();

// Обработка аргументов командной строки
if (args.Contains("--seed"))
{
    Console.WriteLine("Запущено заполнение");
    try
    {
        await app.Services.RunSeeding();
        Console.WriteLine("Заполнение завершено успешно");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при заполнении: {ex.Message}");
        throw;
    }
    return;
}

if (args.Contains("--reset-db"))
{
    try
    {
        await app.Services.EnsureDatabaseDeletedAsync();
        Console.WriteLine("База данных удалена успешно");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при удалении: {ex.Message}");
        throw;
    }
    return;
}

// Инициализация базы данных при старте
try
{
    await app.Services.InitializeDatabaseAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Произошла ошибка при инициализации базы");
    throw;
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tennis Reservation API V1");
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages().WithStaticAssets();

app.Run();