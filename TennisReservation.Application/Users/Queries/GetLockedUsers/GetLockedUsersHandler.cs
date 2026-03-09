using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Users.Dto;

namespace TennisReservation.Application.Users.Queries.GetLockedUsers
{
    public class GetLockedUsersHandler
    {
        private readonly ILogger<GetLockedUsersHandler> _logger;
        private readonly IReadDbContext _readDbContext;

        public GetLockedUsersHandler(ILogger<GetLockedUsersHandler> logger, IReadDbContext readDbContext)
        {
            _logger = logger;
            _readDbContext = readDbContext;
        }

        public async Task<Result<IReadOnlyList<LockedUserDto>>> HandleAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _readDbContext.UserCredentialsRead
                    .Where(uc => uc.LockedUntil !=  null)
                    .Select(uc => new LockedUserDto(
                        uc.UserId.Value,
                        uc.User.FirstName,
                        uc.User.LastName,
                        uc.User.Email,
                        uc.LockedUntil))
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка заблокированных пользователей");
                return Result.Failure<IReadOnlyList<LockedUserDto>>("Не удалось получить список пользователей");
            }
        }
    }
}
