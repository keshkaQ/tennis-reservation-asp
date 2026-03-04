using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Users;
using TennisReservation.Domain.Models;

namespace TennisReservation.Infrastructure.Postgres.Repositories
{
    public class UsersCredentialsRepository : RepositoryBase<UserCredentials, CredentialsId>, IUserCredentialsRepository
    {
        public UsersCredentialsRepository(TennisReservationDbContext dbContext, ILogger<UsersCredentialsRepository> logger)
            : base(dbContext, logger) { }

        public async Task<Result> UpdateAsync(UserCredentials credentials, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync(
                async (transaction) =>
                {
                    _dbSet.Update(credentials);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                },
                successMessage: $"Учётные данные пользователя {credentials.UserId.Value} обновлены",
                errorMessage: $"Ошибка при обновлении учётных данных пользователя {credentials.UserId.Value}",
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

         public async Task<Result<UserCredentials>> GetWithUserByIdAsync(Guid userId,CancellationToken cancellationToken = default)
         {

            return await ExecuteQueryAsync(
                async () =>
                {
                    var user = await _dbContext.Users.Include(u => u.Credentials).FirstOrDefaultAsync(u => u.Id == new UserId(userId), cancellationToken);

                    if (user == null)
                        return null;

                    if (user.Credentials == null)
                    {
                        _logger.LogError("Пользователь {UserId} найден, но учетные данные отсутствуют", user.Id.Value);
                        return null;
                    }

                    return user.Credentials;
                },
                errorMessage: $"Ошибка при получении учетных данных по id {userId}",cancellationToken: cancellationToken);
        }
    }
}