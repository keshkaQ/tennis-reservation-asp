using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Users;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public class UsersRepository : RepositoryBase<User, UserId>, IUsersRepository
    {
        public UsersRepository(TennisReservationDbContext dbContext, ILogger<UsersRepository> logger)
            : base(dbContext, logger) { }

        public async Task<Result<List<User>>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            return await ExecuteQueryAsync(
                async () => await _dbSet
                    .Include(u => u.Reservations)
                    .ToListAsync(cancellationToken),
                errorMessage: "Ошибка при получении списка пользователей",
                cancellationToken: cancellationToken);
        }

        public async Task<Result<User>> GetByIdWithCredentialAsync(UserId id,CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(
                async () => await _dbSet
                    .Include(u => u.Credentials)
                    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken),
                errorMessage: $"Ошибка при получении пользователя {id.Value} с учетными данными",
                cancellationToken: cancellationToken);
        }

        public async Task<Result<User>> GetByIdAsync(UserId id,CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(
                async () => await _dbSet
                    .Include(u => u.Reservations)
                    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken),
                errorMessage: $"Ошибка при получении пользователя {id.Value}",
                cancellationToken: cancellationToken);
        }


        public async Task<Result<User>> GetByEmailAsync(string email,CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(
                async () => await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken),
                errorMessage: $"Ошибка при получении пользователя по email {email}",
                cancellationToken: cancellationToken);
        }

        public async Task<Result<User>> CreateWithCredentialsAsync(User user,CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                async (transaction) =>
                {
                    await _dbSet.AddAsync(user, cancellationToken);

                    if (user.Credentials != null)
                    {
                        await _dbContext.UserCredentials.AddAsync(user.Credentials, cancellationToken);
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return user;
                },
                successMessage: $"Пользователь {user.Id.Value} сохранен в БД с учетными данными",
                errorMessage: $"Не удалось сохранить пользователя {user.Id.Value} с учетными данными",
                cancellationToken: cancellationToken);
        }

        public async Task<Result> UpdateAsync(User user,CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                async (transaction) =>
                {
                    _dbSet.Update(user);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                },
                successMessage: $"Пользователь {user.Id.Value} обновлен",
                errorMessage: $"Не удалось обновить пользователя {user.Id.Value}",
                cancellationToken: cancellationToken);
        }

        public async Task<Result> DeleteWithCredentialsAsync(UserId id,CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                async (transaction) =>
                {
                    var user = await _dbSet
                        .Include(u => u.Credentials)
                        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

                    if (user == null)
                        throw new InvalidOperationException($"Пользователь с ID {id.Value} не найден");

                    _dbSet.Remove(user);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                },
                successMessage: $"Пользователь {id.Value} и его учетные данные успешно удалены",
                errorMessage: $"Не удалось удалить пользователя {id.Value}",
                cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistsAsync(string email,CancellationToken cancellationToken = default)
        {
            return await ExecuteCheckAsync(
                async () => await _dbSet.AnyAsync(u => u.Email == email, cancellationToken),
                errorMessage: $"Ошибка при проверке существования пользователя {email}",
                cancellationToken: cancellationToken);
        }
    }
}