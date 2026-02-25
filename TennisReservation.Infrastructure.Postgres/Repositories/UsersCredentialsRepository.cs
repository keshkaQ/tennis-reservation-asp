using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Users;
using TennisReservation.Domain.Enums;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public class UsersCredentialsRepository : IUserCredentialsRepository
    {
        private readonly ILogger<UsersCredentialsRepository> _logger;
        private readonly TennisReservationDbContext _dbContext;

        public UsersCredentialsRepository(
            TennisReservationDbContext dbContext,
            ILogger<UsersCredentialsRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                email = email?.Trim().ToLower() ?? "";

                var exists = await _dbContext.Users
                    .AnyAsync(u => u.Email == email, cancellationToken);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования email {Email}", email);
                return false;
            }
        }

        public async Task<Result<UserCredentials>> GetByUserIdAsync(UserId userId,CancellationToken cancellationToken = default)
        {
            try
            {
                var credentials = await _dbContext.UserCredentials
                    .Include(c => c.User)  
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (credentials == null)
                {
                    _logger.LogWarning("Учетные данные для пользователя {UserId} не найдены", userId.Value);
                    return Result.Failure<UserCredentials>("Учетные данные не найдены");
                }

                return Result.Success(credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении учетных данных по UserId {UserId}", userId.Value);
                return Result.Failure<UserCredentials>("Ошибка при получении учетных данных");
            }
        }

        public async Task<Result<UserCredentials>> GetWithUserByEmailAsync( string email,CancellationToken cancellationToken = default)
        {
            try
            {
                email = email.Trim().ToLower();

                var user = await _dbContext.Users
                    .Include(u => u.Credentials)
                    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("Пользователь с email {Email} не найден", email);
                    return Result.Failure<UserCredentials>("Пользователь не найден");
                }

                if (user.Credentials == null)
                {
                    _logger.LogError("Пользователь {UserId} найден, но учетные данные отсутствуют", user.Id.Value);
                    return Result.Failure<UserCredentials>("Учетные данные не найдены");
                }

                _logger.LogDebug("Учетные данные для пользователя {UserId} загружены", user.Id.Value);
                return Result.Success(user.Credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении учетных данных по email {Email}", email);
                return Result.Failure<UserCredentials>("Произошла ошибка при загрузке данных");
            }
        }

        public async Task<bool> IsInRoleAsync(UserId userId,UserRole role,CancellationToken cancellationToken = default)
        {
            try
            {
                var credentials = await _dbContext.UserCredentials
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (credentials == null)
                {
                    _logger.LogWarning("Учетные данные для пользователя {UserId} не найдены", userId.Value);
                    return false;
                }

                return credentials.Role == role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке роли пользователя {UserId}", userId.Value);
                return false;
            }
        }

        public async Task<Result> LockUntilAsync(CredentialsId id,DateTime until,CancellationToken cancellationToken = default)
        {
            try
            {
                var credentials = await _dbContext.UserCredentials
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (credentials == null)
                {
                    _logger.LogWarning("Учетные данные с id {CredentialsId} не найдены", id.Value);
                    return Result.Failure("Учетные данные не найдены");
                }

                credentials.LockUntil(until);

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Пользователь {UserId} заблокирован до {Until}", credentials.UserId.Value, until);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при блокировке пользователя {CredentialsId}", id.Value);
                return Result.Failure("Ошибка при блокировке пользователя");
            }
        }

        public async Task<Result> ResetLockoutAsync(CredentialsId id,CancellationToken cancellationToken = default)
        {
            try
            {
                var credentials = await _dbContext.UserCredentials
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (credentials == null)
                {
                    _logger.LogWarning("Учетные данные с id {CredentialsId} не найдены", id.Value);
                    return Result.Failure("Учетные данные не найдены");
                }

                // Сброс блокировки
                credentials.ResetLockout();

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Блокировка сброшена для пользователя {UserId}", credentials.UserId.Value);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сбросе блокировки {CredentialsId}", id.Value);
                return Result.Failure("Ошибка при сбросе блокировки");
            }
        }

        public async Task<Result> UpdateLoginAttemptsAsync(CredentialsId id,bool success,CancellationToken cancellationToken = default)
        {
            try
            {
                var credentials = await _dbContext.UserCredentials
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (credentials == null)
                {
                    _logger.LogWarning("Учетные данные с id {CredentialsId} не найдены", id.Value);
                    return Result.Failure("Учетные данные не найдены");
                }

                if (success)
                {
                    credentials.RecordSuccessfulLogin();
                    _logger.LogDebug("Успешный вход для пользователя {UserId}", credentials.UserId.Value);
                }
                else
                {
                    credentials.RecordFailedAttempt();
                    _logger.LogWarning("Неудачная попытка входа для пользователя {UserId}", credentials.UserId.Value);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении попыток входа {CredentialsId}", id.Value);
                return Result.Failure("Ошибка при обновлении попыток входа");
            }
        }

        public async Task<Result> UpdatePasswordAsync(CredentialsId id,string newPasswordHash,string newSalt,CancellationToken cancellationToken = default)
        {
            try
            {
                var credentials = await _dbContext.UserCredentials
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (credentials == null)
                {
                    _logger.LogWarning("Учетные данные с id {CredentialsId} не найдены", id.Value);
                    return Result.Failure("Учетные данные не найдены");
                }

                credentials.ChangePassword(newPasswordHash, newSalt);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Пароль изменен для пользователя {UserId}", credentials.UserId.Value);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении пароля {CredentialsId}", id.Value);
                return Result.Failure("Ошибка при изменении пароля");
            }
        }

        public async Task<Result> UpdateRoleAsync(CredentialsId id,UserRole newRole,CancellationToken cancellationToken = default)
        {
            try
            {
                var credentials = await _dbContext.UserCredentials
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (credentials == null)
                {
                    _logger.LogWarning("Учетные данные с id {CredentialsId} не найдены", id.Value);
                    return Result.Failure("Учетные данные не найдены");
                }

                credentials.ChangeRole(newRole);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Роль изменена на {Role} для пользователя {UserId}", newRole, credentials.UserId.Value);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении роли {CredentialsId}", id.Value);
                return Result.Failure("Ошибка при изменении роли");
            }
        }
    }
}