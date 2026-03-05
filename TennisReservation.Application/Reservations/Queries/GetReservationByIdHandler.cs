using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Contracts.Reservations.Queries;
using TennisReservation.Domain.Models;

public class GetReservationByIdHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetReservationByIdHandler> _logger;

    public GetReservationByIdHandler(IReadDbContext readDbContext, ILogger<GetReservationByIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<ReservationDto?>> HandleAsync(GetReservationByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _readDbContext.ReservationsRead
                .Where(r => r.Id == new ReservationId(query.Id))
                .Select(r => new ReservationDto(
                    r.Id.Value,
                    r.TennisCourtId.Value,
                    r.UserId.Value,
                    r.StartTime,
                    r.EndTime,
                    r.TotalCost,
                    r.Status
                )).FirstOrDefaultAsync(cancellationToken);

            if (dto is null)
                return Result.Failure<ReservationDto?>("Бронирование не найдено");

            return Result.Success<ReservationDto?>(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении бронирования {ReservationId}", query.Id);
            return Result.Failure<ReservationDto?>("Не удалось получить бронирование");
        }
    }
}