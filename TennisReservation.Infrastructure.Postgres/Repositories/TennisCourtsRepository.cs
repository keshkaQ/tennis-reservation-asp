using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.TennisCourts;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public class TennisCourtsRepository : ITennisCourtsRepository
    {
        private readonly ILogger<TennisCourtsRepository> _logger;
        private readonly TennisReservationDbContext _dbContext;

        public TennisCourtsRepository(TennisReservationDbContext dbContext, ILogger<TennisCourtsRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<Result<TennisCourt>> CreateAsync(TennisCourt tennisCourt, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _dbContext.TennisCourts.AddAsync(tennisCourt, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    _logger.LogInformation("Корт {TennisCourtId} сохранен в БД", tennisCourt.Id.Value);
                    return Result.Success(tennisCourt);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при сохранении корта {TennisCourtId}", tennisCourt.Id.Value);
                return Result.Failure<TennisCourt>("Не удалось сохранить корт в БД");
            }
           
        }

        public async Task<Result> DeleteAsync(TennisCourtId id, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var transaction = await _dbContext.Database
              .BeginTransactionAsync(cancellationToken);

                try
                {
                    var tennisCourt = await _dbContext.TennisCourts
                        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

                    if (tennisCourt == null)
                        return Result.Failure($"Корт с ID {id.Value} не найден");
                    _logger.LogInformation(
                        "Удаление корта {TennisCourtId}", id.Value);

                    _dbContext.TennisCourts.Remove(tennisCourt);

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Корт {TennisCourtId} успешно удален ", id.Value);

                    return Result.Success();
                }
                catch
                {
                    // Откатываем транзакцию в случае ошибки
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Не удалось удалить корт {TennisCourtId}", id.Value);
                return Result.Failure("Не удалось удалить корт");
            }
           
        }

        public async Task<Result<TennisCourt>> GetByIdAsync(TennisCourtId id, CancellationToken cancellationToken)
        {
            var tennisCourt = await _dbContext.TennisCourts
                .FirstOrDefaultAsync(tc => tc.Id == id, cancellationToken);
            if (tennisCourt == null)
                return Result.Failure<TennisCourt>($"Корт с {id} не найден");
            return Result.Success(tennisCourt);
        }

        public async Task<Result<TennisCourt>> GetByIdWithReservationsAsync(TennisCourtId id, CancellationToken cancellationToken)
        {
            var tennisCourt = await _dbContext.TennisCourts
                    .Include(tc => tc.Reservations)  
                    .FirstOrDefaultAsync(tc => tc.Id == id, cancellationToken);
            if (tennisCourt == null)
                return Result.Failure<TennisCourt>($"Корт с {id} не найден");
            return Result.Success(tennisCourt);
        }

        public async Task<Result> UpdateAsync(TennisCourt tennisCourt, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.TennisCourts.Update(tennisCourt);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Не удалось обновить корт");

                return Result.Failure<Guid>("Не удалось обновить корт");
            }
        }
    }
}
