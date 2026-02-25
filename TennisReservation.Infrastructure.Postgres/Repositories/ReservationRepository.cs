using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Reservations;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public class ReservationRepository : RepositoryBase<Reservation,ReservationId>, IReservationRepository
    {
        public ReservationRepository(TennisReservationDbContext dbContext, ILogger<ReservationRepository> logger)
             : base(dbContext, logger) { }
        public async Task<Result<Reservation>> CreateAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                async (transaction) =>
                {
                    await _dbSet.AddAsync(reservation, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return reservation;
                },
                successMessage: $"Бронирование {reservation.Id.Value} сохранено в БД",
                errorMessage: $"Не удалось сохранить бронирование {reservation.Id.Value}",
                cancellationToken: cancellationToken);
        }

        public async Task<Result> UpdateAsync(Reservation reservation,CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                 async (transaction) =>
                 {
                     _dbSet.Update(reservation);
                     await _dbContext.SaveChangesAsync(cancellationToken);
                 },
                 successMessage: $"Бронирование {reservation.Id.Value} обновлено",
                 errorMessage: $"Не удалось обновить бронирование {reservation.Id.Value}",
                 cancellationToken: cancellationToken);
        }

        public async Task<Result> DeleteAsync(ReservationId id, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
               async (transaction) =>
               {
                   var reservation = await _dbSet.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                   if (reservation == null)
                       throw new InvalidOperationException($"Бронирование с ID {id.Value} не найдено");

                   _dbSet.Remove(reservation);
                   await _dbContext.SaveChangesAsync(cancellationToken);
               },
               successMessage: $"Бронирование {id.Value} успешно удалено",
               errorMessage: $"Не удалось удалить бронирование {id.Value}",
               cancellationToken: cancellationToken);
        }

        public async Task<Result<Reservation>> GetByIdAsync(ReservationId id, CancellationToken cancellationToken)
        {
            return await ExecuteQueryAsync(
               async () => await _dbSet.FirstOrDefaultAsync(r => r.Id == id, cancellationToken),
               errorMessage: $"Ошибка при получении бронирования {id.Value}",
               cancellationToken: cancellationToken);
        }

        public async Task<bool> CheckAvailabilityAsync(Guid tennisCourtId, DateTime startTime, DateTime endTime, Guid? excludeReservationId = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteCheckAsync(
                 async () =>
                 {
                     var query = _dbSet
                         .Where(r => r.TennisCourtId.Value == tennisCourtId)
                         .Where(r =>
                             (startTime >= r.StartTime && startTime < r.EndTime) ||
                             (endTime > r.StartTime && endTime <= r.EndTime) ||
                             (startTime <= r.StartTime && endTime >= r.EndTime)
                         );

                     if (excludeReservationId.HasValue)
                     {
                         query = query.Where(r => r.Id.Value != excludeReservationId.Value);
                     }

                     return !await query.AnyAsync(cancellationToken);
                 },
                 errorMessage: $"Ошибка при проверке доступности корта {tennisCourtId}",
                 cancellationToken: cancellationToken);
        }
    }
}
