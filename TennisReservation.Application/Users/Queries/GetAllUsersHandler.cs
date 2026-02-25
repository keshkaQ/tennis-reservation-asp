using TennisReservation.Application.Database;
using TennisReservation.Contracts.Users.Dto;
using Microsoft.EntityFrameworkCore;
using CSharpFunctionalExtensions;

namespace TennisReservation.Application.Users.Queries
{
    public class GetAllUsersHandler
    {
        private readonly IReadDbContext _readDbContext;
        public GetAllUsersHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }
        public async Task<Result<List<UserDto>>> HandleAsync(CancellationToken cancellationToken)
        {
            return await _readDbContext.UsersRead
                .Select(user => new UserDto(
                    user.Id.Value,             
                    user.FirstName,              
                    user.LastName,            
                    user.Email,                  
                    user.PhoneNumber,           
                    user.RegistrationDate,    
                    user.Reservations.Count     
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
