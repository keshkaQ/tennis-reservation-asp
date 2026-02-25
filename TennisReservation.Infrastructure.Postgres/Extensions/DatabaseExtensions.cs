using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TennisReservation.Infrastructure.Postgres.Seeding;

namespace TennisReservation.Infrastructure.Postgres.Extensions
{
    public static class DatabaseExtensions
    {
        public static async Task InitializeDatabaseAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TennisReservationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TennisReservationDbContext>>();

            try
            {
                // 1. СНАЧАЛА применяем миграции (создаем таблицы)
                logger.LogInformation("Применяем миграции");
                await context.Database.MigrateAsync();

                bool hasData = false;
                try
                {
                    hasData = await context.Users.AnyAsync();
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Таблицы еще не содержат данных или не существуют");
                    hasData = false;
                }

                // 3. Если данных нет - заполняем
                if (!hasData)
                {
                    logger.LogInformation("База данных пустая, начинаем заполнение");
                    await services.RunSeeding();
                    logger.LogInformation("Заполнение прошло успешно");
                }
                else
                {
                    logger.LogInformation("База данных уже содержит данные, пропускаем заполнение");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка во время инициализации базы данных");
                throw;
            }
        }

        public static async Task EnsureDatabaseDeletedAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TennisReservationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TennisReservationDbContext>>();

            try
            {
                logger.LogWarning("Удаляем базу данных");
                await context.Database.EnsureDeletedAsync();
                logger.LogInformation("База данных удалена успешно");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при удалении базы данных");
                throw;
            }
        }
    }
}