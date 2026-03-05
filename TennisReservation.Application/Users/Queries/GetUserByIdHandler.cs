using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Users.Dto;
using TennisReservation.Contracts.Users.Queries;
using TennisReservation.Domain.Models;

public class GetUserByIdHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetUserByIdHandler> _logger;

    public GetUserByIdHandler(IReadDbContext readDbContext, ILogger<GetUserByIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<UserDto?>> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _readDbContext.UsersRead
                .Where(u => u.Id == new UserId(query.UserId))
                .Select(user => new UserDto(
                    user.Id.Value,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.RegistrationDate,
                    user.Reservations.Count()
                )).FirstOrDefaultAsync(cancellationToken);

            return Result.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пользователя {UserId}", query.UserId);
            return Result.Failure<UserDto?>("Не удалось получить пользователя");
        }
    }
}