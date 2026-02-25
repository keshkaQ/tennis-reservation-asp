using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Reservations;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ILogger<ReservationRepository> _logger;
        private readonly TennisReservationDbContext _dbContext;
        public ReservationRepository(TennisReservationDbContext dbContext, ILogger<ReservationRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<Result<Reservation>> CreateAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _dbContext.Reservations.AddAsync(reservation, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    _logger.LogInformation("Бронирование {ReservationId} сохранено в БД", reservation.Id.Value);
                    return Result.Success(reservation);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении бронирования {ReservationId}", reservation.Id.Value);
                return Result.Failure<Reservation>("Не удалось сохранить бронирование в БД");
            }
        }

        public async Task<Result> UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.Reservations.Update(reservation);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось обновить бронирование");

                return Result.Failure<Guid>("Не удалось обновить бронирование");
            }
        }

        public async Task<Result> DeleteAsync(ReservationId id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var transaction = await _dbContext.Database
               .BeginTransactionAsync(cancellationToken);

                try
                {
                    var reservation = await _dbContext.Reservations
                        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

                    if (reservation == null)
                        return Result.Failure($"Бронирование с ID {id.Value} не найдено");
                    _logger.LogInformation(
                        "Удаление бронирования {ReservationId}", id.Value);

                    _dbContext.Reservations.Remove(reservation);

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Бронирование {ReservationId} успешно удалено", id.Value);

                    return Result.Success();
                }
                catch
                {
                    // Откатываем транзакцию в случае ошибки
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось удалить бронирование {ReservationId}", id.Value);
                return Result.Failure("Не удалось удалить бронирование");
            }
        }

        public async Task<Result<Reservation>> GetByIdAsync(ReservationId id, CancellationToken cancellationToken)
        {
            var reservation = await _dbContext.Reservations
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            if (reservation == null)
                return Result.Failure<Reservation>($"Бронирование с {id} не найдено");
            return Result.Success(reservation);
        }

        public async Task<bool> CheckAvailabilityAsync(Guid tennisCourtId, DateTime startTime, DateTime endTime, Guid? excludeReservationId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Reservations
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

        }
    }
}
