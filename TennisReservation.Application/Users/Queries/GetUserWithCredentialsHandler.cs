using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Users.Queries;
using TennisReservation.Domain.Enums;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Users.Queries
{
    public class GetUserWithCredentialsHandler
    {
        private readonly IReadDbContext _readDbContext;
        public GetUserWithCredentialsHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }
        public async Task<Result<UserWithCredentialsDto?>> HandleAsync(GetUserWithCredentialsByIdQuery query, CancellationToken cancellationToken)
        {
            return await _readDbContext.UsersRead
                 .Where(user => user.Id == new UserId(query.Id))
                 .Include(u => u.Credentials)
                 .Select(u => new UserWithCredentialsDto
                 (
                     u.Id.Value,
                     u.FirstName,
                     u.LastName,
                     u.Email,
                     u.PhoneNumber,
                     u.RegistrationDate,
                     u.Reservations.Count(),

                     // Поля из Credentials
                     u.Credentials != null ? u.Credentials.Role : UserRole.User,
                     u.Credentials != null ? u.Credentials.LastLoginAt : null,
                     u.Credentials != null ? u.Credentials.FailedLoginAttempts : 0,
                     u.Credentials != null ? u.Credentials != null && u.Credentials.LockedUntil.HasValue && u.Credentials.LockedUntil.Value > DateTime.UtcNow : false,
                     u.Credentials != null ? u.Credentials.LockedUntil : null
                 )).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
