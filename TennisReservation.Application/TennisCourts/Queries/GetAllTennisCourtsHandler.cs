using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.TennisCourts.DTO;

public class GetAllTennisCourtsHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetAllTennisCourtsHandler> _logger;

    public GetAllTennisCourtsHandler(IReadDbContext readDbContext, ILogger<GetAllTennisCourtsHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<List<TennisCourtDto>>> HandleAsync(CancellationToken cancellationToken)
    {
        try
        {
            var courts = await _readDbContext.TennisCourtsRead
                .Select(tc => new TennisCourtDto(
                    tc.Id.Value,
                    tc.Name,
                    tc.HourlyRate,
                    tc.Description
                )).ToListAsync(cancellationToken);

            return Result.Success(courts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка кортов");
            return Result.Failure<List<TennisCourtDto>>("Не удалось получить список кортов");
        }
    }
}