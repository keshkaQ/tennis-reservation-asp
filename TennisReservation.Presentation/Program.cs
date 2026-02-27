using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;
using TennisReservation.Application.Auth;
using TennisReservation.Application.Database;
using TennisReservation.Application.Interfaces;
using TennisReservation.Application.Reservations;
using TennisReservation.Application.Reservations.Commands;
using TennisReservation.Application.Reservations.Queries;
using TennisReservation.Application.Statictics;
using TennisReservation.Application.TennisCourts;
using TennisReservation.Application.TennisCourts.Commands;
using TennisReservation.Application.TennisCourts.Queries;
using TennisReservation.Application.Users;
using TennisReservation.Application.Users.Commands;
using TennisReservation.Application.Users.Queries;
using TennisReservation.Infrastructure.Postgres;
using TennisReservation.Infrastructure.Postgres.Extensions;
using TennisReservation.Infrastructure.Postgres.Repositories;
using TennisReservation.Infrastructure.Postgres.Seeding;
using TennisReservation.Infrastructure.Postgres.Services;
using TennisReservation.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Всегда преобразовывать enum в строки в API
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }); 
builder.Services.AddRazorPages();              // razor pages
builder.Services.AddEndpointsApiExplorer();

// swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tennis Reservation API",
        Version = "v1",
        Description = "API для управления теннисным клубом"
    });
});

// логгер
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

// База данных для записей и чтения
builder.Services.AddDbContext<TennisReservationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("TennisReservationDb"));
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.UseLoggerFactory(TennisReservationDbContext.CreateLoggerFactory());
});

builder.Services.AddScoped<IReadDbContext>(sp =>
    sp.GetRequiredService<TennisReservationDbContext>());

// Регистрация репозиториев
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUserCredentialsRepository, UsersCredentialsRepository>();
builder.Services.AddScoped<ITennisCourtsRepository, TennisCourtsRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// Регистрация PasswordHasher, JwtProvider
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

// Регистрация фич юзера
builder.Services.AddScoped<GetAllUsersHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<GetUserByEmailHandler>();
builder.Services.AddScoped<GetUserWithCredentialsHandler>();
builder.Services.AddScoped<CreateUserWithCredentialsHandler>();
builder.Services.AddScoped<GetStatisticsHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();

// Регистрация фич кортов
builder.Services.AddScoped<GetAllTennisCourtsHandler>();
builder.Services.AddScoped<GetTennisCourtByIdHandler>();
builder.Services.AddScoped<CreateTennisCourtHandler>();
builder.Services.AddScoped<UpdateTennisCourtHandler>();
builder.Services.AddScoped<DeleteTennisCourtHandler>();

// Регистрация фич бронирований
builder.Services.AddScoped<GetAllReservationsHandler>();
builder.Services.AddScoped<GetReservationByIdHandler>();
builder.Services.AddScoped<CreateReservationHandler>();
builder.Services.AddScoped<UpdateReservationHandler>();
builder.Services.AddScoped<DeleteReservationHandler>();

// Регистрация сервисов для аутентификации
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GetUserWithCredentialsByEmailHandler>();

builder.Services.AddApiAuthentification(Options.Create(builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()));


// Регистрация seeder
builder.Services.AddScoped<ISeeder, ReservationSeeder>();

var app = builder.Build();

// Применяем команды для заполнения
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


// Заполняем бд при старте
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


// методы защиты, чтобы js не мог прочитать cookie, cookie отправка только по https
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages().WithStaticAssets();

app.Run();



// проверить api на эти методы

// Интерфейс:
// создать полноценную  админ-панель, где можно снять блокировку, заблокировать, изменить пароль + существующий crud
// На главном экране сделать страницу, где будут корты и их расписание и кнопка забронировать