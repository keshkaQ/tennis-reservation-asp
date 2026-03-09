using CSharpFunctionalExtensions;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Users.Interfaces
{
    public interface IUserCredentialsRepository
    {
        Task<Result<UserCredentials>> GetWithUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(UserCredentials credentials, CancellationToken cancellationToken = default);
        Task<Result<UserCredentials>> GetWithUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}