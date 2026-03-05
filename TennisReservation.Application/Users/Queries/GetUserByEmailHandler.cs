using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Contracts.Users.Queries;

public class GetUserByEmailHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetUserByEmailHandler> _logger;

    public GetUserByEmailHandler(IReadDbContext readDbContext, ILogger<GetUserByEmailHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<UserDto?>> HandleAsync(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _readDbContext.UsersRead
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

            return Result.Success<UserDto?>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пользователя по email {Email}", query.Email);
            return Result.Failure<UserDto?>("Не удалось получить пользователя");
        }
    }
}