using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Contracts.Users.Queries;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Users.Queries
{
    public class GetUserByIdHandler
    {
        private readonly IReadDbContext _readDbContext;
        public GetUserByIdHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }
        public async Task<Result<UserDto?>> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            return await _readDbContext.UsersRead
                .Where(u => u.Id == new UserId(query.UserId))
                .Select(user => new UserDto
                (
                    user.Id.Value,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.RegistrationDate,
                    user.Reservations.Count()
                )).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
