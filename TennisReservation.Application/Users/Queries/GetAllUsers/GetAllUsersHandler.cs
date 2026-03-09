using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Users.Dto;

public class GetAllUsersHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetAllUsersHandler> _logger;

    public GetAllUsersHandler(IReadDbContext readDbContext, ILogger<GetAllUsersHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<List<UserDto>>> HandleAsync(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _readDbContext.UsersRead
                .Select(user => new UserDto(
                    user.Id.Value,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.RegistrationDate,
                    user.Reservations.Count
                )).ToListAsync(cancellationToken);

            return Result.Success(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка пользователей");
            return Result.Failure<List<UserDto>>("Не удалось получить список пользователей");
        }
    }
}