using CSharpFunctionalExtensions;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Users
{
    public interface IUsersRepository
    {
        // Read-методы - всегда с токеном (без default)
        Task<Result<List<User>>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<Result<User>> GetByIdAsync(UserId id, CancellationToken cancellationToken);
        Task<Result<User>> GetByIdWithCredentialAsync(UserId id, CancellationToken cancellationToken);
        Task<Result<User>> GetByEmailAsync(string email, CancellationToken cancellationToken);

        // Быстрая проверка - можно default
        Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);

        // Write-методы - с default 
        Task<Result<User>> CreateWithCredentialsAsync(User user, CancellationToken cancellationToken = default);
        Task<Result<Guid>> UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task<Result> DeleteWithCredentialsAsync(UserId id, CancellationToken cancellationToken = default);
    }
}