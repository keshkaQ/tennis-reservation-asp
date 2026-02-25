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
        Task<Result> UpdateLoginAttemptsAsync(CredentialsId id, bool success, CancellationToken cancellationToken = default);
        Task<Result> ResetLockoutAsync(CredentialsId id, CancellationToken cancellationToken = default);
        Task<Result> LockUntilAsync(CredentialsId id, DateTime until, CancellationToken cancellationToken = default);
        Task<Result> UpdateRoleAsync(CredentialsId id, UserRole newRole, CancellationToken cancellationToken = default);
        Task<bool> IsInRoleAsync(UserId userId, UserRole role, CancellationToken cancellationToken = default);
        Task<Result> UpdatePasswordAsync(CredentialsId id, string newPasswordHash, string newSalt, CancellationToken cancellationToken = default);
    }
}