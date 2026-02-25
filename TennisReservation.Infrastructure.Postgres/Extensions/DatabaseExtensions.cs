using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TennisReservation.Infrastructure.Postgres.Seeding;

namespace TennisReservation.Infrastructure.Postgres.Extensions
{
    public static class DatabaseExtensions
    {
        // Инициализирует БД -  создает таблицы и заполняет если нужно
        public static async Task InitializeDatabaseAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TennisReservationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TennisReservationDbContext>>();
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            try
            {
                // 1. СНАЧАЛА применяем миграции (создаем таблицы)
                logger.LogInformation("Применяем миграции БД");
                await context.Database.MigrateAsync();
                logger.LogInformation("Миграции применены успешно");

                // 2. Проверяем есть ли данные
                bool hasData = false;
                try
                {
                    hasData = await context.Users.AnyAsync();
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Не удалось проверить наличие данных в БД");
                    hasData = false;
                }

                if (!hasData)
                {
                    if (environment.IsDevelopment())
                    {
                        logger.LogInformation("БД пустая (Development среда). Начинаем заполнение");
                        await services.RunSeeding();
                        logger.LogInformation("БД заполнена успешно");
                    }
                    else
                    {

                        throw new InvalidOperationException($"База данных пустая в {environment.EnvironmentName} окружении");
                    }
                }
                else
                {
                    logger.LogInformation("БД уже содержит данные, пропускаем заполнение");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при инициализации БД");
                throw;
            }
        }

        // Удаляет всю БД полностью
        public static async Task EnsureDatabaseDeletedAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TennisReservationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TennisReservationDbContext>>();
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            try
            {
                if (!environment.IsDevelopment())
                {
                    logger.LogError(" Попытка удалить БД в {Environment} окружении", environment.EnvironmentName);
                    throw new InvalidOperationException($"Нельзя удалять БД в {environment.EnvironmentName} окружении");
                }

                logger.LogWarning("Удаляем базу данных");
                await context.Database.EnsureDeletedAsync();
                logger.LogInformation("База данных удалена успешно");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при удалении БД");
                throw;
            }
        }
    }
}