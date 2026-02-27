using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Contracts.Users.Queries;
using TennisReservation.Domain.Enums;

namespace TennisReservation.Application.Users.Queries
{
    public class GetUserWithCredentialsByEmailHandler
    {
        private readonly IReadDbContext _readDbContext;

        public GetUserWithCredentialsByEmailHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async Task<Result<UserLoginDto?>> HandleAsync(GetUserWithCredentialsByEmailQuery query,CancellationToken cancellationToken)
        {
            return await _readDbContext.UsersRead
                .Where(user => user.Email == query.Email)
                .Include(u => u.Credentials)
                .Select(u => new UserLoginDto
                 (
                     u.Id.Value,
                     u.Email,
                     u.Credentials != null ? u.Credentials.Role : UserRole.User,
                     u.Credentials != null ? u.Credentials.PasswordHash : string.Empty
                 )).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
