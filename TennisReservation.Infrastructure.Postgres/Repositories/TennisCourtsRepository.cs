using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.TennisCourts;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public class TennisCourtsRepository : RepositoryBase<TennisCourt, TennisCourtId>, ITennisCourtsRepository
    {
        public TennisCourtsRepository(TennisReservationDbContext dbContext, ILogger<TennisCourtsRepository> logger)
            : base(dbContext, logger) { }


        public async Task<Result<TennisCourt>> CreateAsync(TennisCourt tennisCourt,CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                async (transaction) =>
                {
                    await _dbSet.AddAsync(tennisCourt, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return tennisCourt;
                },
                successMessage: $"Корт {tennisCourt.Id.Value} сохранен в БД",
                errorMessage: $"Не удалось сохранить корт {tennisCourt.Id.Value}",
                cancellationToken: cancellationToken);
        }

        public async Task<Result> DeleteAsync(TennisCourtId id,CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                async (transaction) =>
                {
                    var tennisCourt = await _dbSet.FirstOrDefaultAsync(tc => tc.Id == id, cancellationToken);

                    if (tennisCourt == null)
                        throw new InvalidOperationException($"Корт с ID {id.Value} не найден");

                    _dbSet.Remove(tennisCourt);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                },
                successMessage: $"Корт {id.Value} успешно удален",
                errorMessage: $"Не удалось удалить корт {id.Value}",
                cancellationToken: cancellationToken);
        }

        public async Task<Result<TennisCourt>> GetByIdAsync(TennisCourtId id,CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(
                async () => await _dbSet.FirstOrDefaultAsync(tc => tc.Id == id, cancellationToken),
                errorMessage: $"Ошибка при получении корта {id.Value}",
                cancellationToken: cancellationToken);
        }

        public async Task<Result<TennisCourt>> GetByIdWithReservationsAsync(TennisCourtId id,CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(
                async () => await _dbSet
                    .Include(tc => tc.Reservations)
                    .FirstOrDefaultAsync(tc => tc.Id == id, cancellationToken),
                errorMessage: $"Ошибка при получении корта {id.Value} с бронированиями",
                cancellationToken: cancellationToken);
        }

        public async Task<Result> UpdateAsync(TennisCourt tennisCourt,CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                async (transaction) =>
                {
                    _dbSet.Update(tennisCourt);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                },
                successMessage: $"Корт {tennisCourt.Id.Value} обновлен",
                errorMessage: $"Не удалось обновить корт {tennisCourt.Id.Value}",
                cancellationToken: cancellationToken);
        }
    }
}
