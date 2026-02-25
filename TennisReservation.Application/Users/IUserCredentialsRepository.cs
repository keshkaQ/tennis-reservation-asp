using CSharpFunctionalExtensions;
using TennisReservation.Domain.Enums;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Users
{
    public interface IUserCredentialsRepository
    {
        Task<Result<UserCredentials>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
        Task<Result<UserCredentials>> GetWithUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsInRoleAsync(UserId userId, UserRole role, CancellationToken cancellationToken = default);
    }
}