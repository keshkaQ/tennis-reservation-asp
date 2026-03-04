using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TennisReservation.Domain.Enums;
using TennisReservation.Infrastructure.Postgres;

/// <summary>
/// Обновление статусов бронирований во время работы приложения, запускается отдельный поток на фоне
/// 
/// Что происходит каждую минуту:
/// 1. CreateScope() — создаём новый контекст как для HTTP запроса
/// 2. Запрос к БД - находим брони с устаревшим статусом
/// 3. Меняем статусы
/// 4. Сохраняем в БД новые статусы одной транзакцией
/// 5. Освобождается DbContext, через минуту повторяем запрос
/// </summary>

public class ReservationStatusUpdater : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationStatusUpdater> _logger;

    public ReservationStatusUpdater(IServiceScopeFactory scopeFactory, ILogger<ReservationStatusUpdater> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // цикл крутится пока приложение не остановится
        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateStatusesAsync();                               // делаем работу
           
            // ждём минуту — не блокируя поток
            // если приложение останавливается во время ожидания — stoppingToken отменит Task.Delay и цикл завершится
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task UpdateStatusesAsync()
    {
        // создаём временный scope имитируя запрос
        using var scope = _scopeFactory.CreateScope();

        // получаем DbContext внутри этого scope
        var dbContext = scope.ServiceProvider.GetRequiredService<TennisReservationDbContext>();

        var now = DateTime.UtcNow;

        // Booked → Active (началось но не закончилось)
        var toActive = await dbContext.Reservations
            .Where(r => r.Status == ReservationStatus.Booked
                     && r.StartTime <= now
                     && r.EndTime > now)
            .ToListAsync();

        // Active/Booked → Completed (закончилось)
        var toCompleted = await dbContext.Reservations
            .Where(r => (r.Status == ReservationStatus.Active || r.Status == ReservationStatus.Booked)
                     && r.EndTime <= now)
            .ToListAsync();

        foreach (var r in toActive)
            r.Activate(); 

        foreach (var r in toCompleted)
            r.Complete(); 

        if (toActive.Any() || toCompleted.Any())
        {
            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Статусы обновлены: {Active} активных, {Completed} завершённых",toActive.Count, toCompleted.Count);
        }
    }
}