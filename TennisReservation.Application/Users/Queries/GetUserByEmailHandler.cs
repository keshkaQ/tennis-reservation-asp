using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Contracts.Users.Queries;

namespace TennisReservation.Application.Users.Queries
{
    public class GetUserByEmailHandler
    {
        private readonly IReadDbContext _readDbContext;
        public GetUserByEmailHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }
        public async Task<UserDto?> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
        {
            return await _readDbContext.UsersRead
                .Where(u => u.Email == query.Email)
                .Select(user => new UserDto(
                    user.Id.Value,              
                    user.FirstName,             
                    user.LastName,               
                    user.Email,                 
                    user.PhoneNumber,           
                    user.RegistrationDate,      
                    user.Reservations.Count       
                )).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
