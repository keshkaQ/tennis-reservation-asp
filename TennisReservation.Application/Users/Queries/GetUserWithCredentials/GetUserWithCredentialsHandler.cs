using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Application.Users.Queries.GetUserWithCredentials;
using TennisReservation.Domain.Enums;
using TennisReservation.Domain.Models;

public class GetUserWithCredentialsHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetUserWithCredentialsHandler> _logger;

    public GetUserWithCredentialsHandler(IReadDbContext readDbContext, ILogger<GetUserWithCredentialsHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<UserWithCredentialsDto?>> HandleAsync(GetUserWithCredentialsByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _readDbContext.UsersRead
                .Where(u => u.Id == new UserId(query.Id))
                .Include(u => u.Credentials)
                .Select(u => new UserWithCredentialsDto(
                    u.Id.Value,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PhoneNumber,
                    u.RegistrationDate,
                    u.Reservations.Count(),
                    u.Credentials != null ? u.Credentials.Role : UserRole.User,
                    u.Credentials != null ? u.Credentials.LastLoginAt : null,
                    u.Credentials != null ? u.Credentials.FailedLoginAttempts : 0,
                    u.Credentials != null && u.Credentials.LockedUntil.HasValue && u.Credentials.LockedUntil.Value > DateTime.UtcNow,
                    u.Credentials != null ? u.Credentials.LockedUntil : null
                )).FirstOrDefaultAsync(cancellationToken);

            return Result.Success<UserWithCredentialsDto?>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пользователя с учетными данными {UserId}", query.Id);
            return Result.Failure<UserWithCredentialsDto?>("Не удалось получить данные пользователя");
        }
    }
}