using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Users;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ILogger<UsersRepository> _logger;
        private readonly TennisReservationDbContext _dbContext;

        public UsersRepository(TennisReservationDbContext dbContext, ILogger<UsersRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<Result<List<User>>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            try
            {
                var users = await _dbContext.Users
                            .Include(u => u.Reservations)  
                            .ToListAsync(cancellationToken);
                return Result.Success(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей");
                return Result.Failure<List<User>>("Не удалось получить список пользователей");
            }
        }
        public async Task<Result<User>> GetByIdWithCredentialAsync(UserId id, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Include(u => u.Credentials)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user == null)
            {
                return Result.Failure<User>("Пользователь не найден");
            }
            return Result.Success(user);
        }

        public async Task<Result<User>> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                 .Include(u => u.Reservations)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user == null)
            {
                return Result.Failure<User>("Пользователь не найден");
            }
            return Result.Success(user);
        }


        public async Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

                if (user == null)
                    return Result.Failure<User>("Пользователь не найден");

                return Result.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя по email {Email}", email);
                return Result.Failure<User>("Ошибка при получении пользователя");
            }
        }

        public async Task<Result<User>> CreateWithCredentialsAsync(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var transaction = await _dbContext.Database
                    .BeginTransactionAsync(cancellationToken);

                try
                {
                    await _dbContext.Users.AddAsync(user, cancellationToken);

                    if (user.Credentials != null)
                    {
                        await _dbContext.UserCredentials.AddAsync(user.Credentials, cancellationToken);
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Пользователь {UserId} сохранен в БД", user.Id.Value);
                    return Result.Success(user);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении пользователя {UserId}", user.Id.Value);
                return Result.Failure<User>("Не удалось сохранить пользователя в БД");
            }
        }
        public async Task<Result<Guid>> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return user.Id.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось обновить пользователя");

                return Result.Failure<Guid>("Не удалось обновить пользователя");
            }
        }

        public async Task<Result> DeleteWithCredentialsAsync(UserId id,CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.Credentials)  
                    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

                if (user == null)
                    return Result.Failure($"Пользователь с ID {id.Value} не найден");
                _logger.LogInformation(
                    "Удаление пользователя {UserId} с email {Email}",
                    id.Value,
                    user.Email);

                _dbContext.Users.Remove(user);

                var result = await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Пользователь {UserId} и его учетные данные успешно удалены " +id.Value);

                return Result.Success();
            }
            catch (Exception ex)
            {
                // Откатываем транзакцию в случае ошибки
                await transaction.RollbackAsync(cancellationToken);

                _logger.LogError(
                    ex,
                    "Не удалось удалить пользователя {UserId} с учетными данными",
                    id.Value);

                return Result.Failure("Не удалось удалить пользователя");
            }
        }

        public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken)
        {
            try
            {
                return await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования пользователя {Email}", email);
                return false;
            }
        }

    }
}
