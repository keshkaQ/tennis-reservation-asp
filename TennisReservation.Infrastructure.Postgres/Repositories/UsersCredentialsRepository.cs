using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Users;
using TennisReservation.Domain.Enums;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public class UsersCredentialsRepository : RepositoryBase<UserCredentials, CredentialsId>, IUserCredentialsRepository
    {
        public UsersCredentialsRepository(TennisReservationDbContext dbContext, ILogger<UsersCredentialsRepository> logger)
            : base(dbContext, logger) { }

        public async Task<bool> ExistsByEmailAsync(string email,CancellationToken cancellationToken = default)
        {
            email = email?.Trim().ToLower() ?? "";

            return await ExecuteCheckAsync(
                async () => await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken),
                errorMessage: $"Ошибка при проверке существования email {email}",
                cancellationToken: cancellationToken);
        }

        public async Task<Result<UserCredentials>> GetByUserIdAsync(UserId userId,CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync(
                async () => await _dbSet.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken),
                errorMessage: $"Ошибка при получении учетных данных для пользователя {userId.Value}",
                cancellationToken: cancellationToken);
        }

        public async Task<Result<UserCredentials>> GetWithUserByEmailAsync(string email,CancellationToken cancellationToken = default)
        {
            email = email.Trim().ToLower();

            return await ExecuteQueryAsync(
                async () =>
                {
                    var user = await _dbContext.Users.Include(u => u.Credentials).FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

                    if (user == null)
                        return null;

                    if (user.Credentials == null)
                    {
                        _logger.LogError("Пользователь {UserId} найден, но учетные данные отсутствуют", user.Id.Value);
                        return null;
                    }

                    return user.Credentials;
                },
                errorMessage: $"Ошибка при получении учетных данных по email {email}",
                cancellationToken: cancellationToken);
        }

        public async Task<bool> IsInRoleAsync(UserId userId,UserRole role,CancellationToken cancellationToken = default)
        {
            return await ExecuteCheckAsync(
                async () =>
                {
                    var credentials = await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
                    return credentials != null && credentials.Role == role;
                },
                errorMessage: $"Ошибка при проверке роли пользователя {userId.Value}",
                cancellationToken: cancellationToken);
        }
    }
}