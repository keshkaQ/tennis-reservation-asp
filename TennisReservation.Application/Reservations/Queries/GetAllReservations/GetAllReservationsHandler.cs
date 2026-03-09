using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Reservations.DTO;

public class GetAllReservationsHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetAllReservationsHandler> _logger;

    public GetAllReservationsHandler(IReadDbContext readDbContext, ILogger<GetAllReservationsHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<ReservationListItemDto>> HandleAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _readDbContext.ReservationsRead
                .OrderByDescending(r => r.StartTime)
                .Select(r => new ReservationListItemDto(
                    r.Id.Value,
                    r.TennisCourtId.Value,
                    r.TennisCourt.Name,
                    r.UserId.Value,
                    r.User.FirstName,
                    r.User.LastName,
                    r.StartTime,
                    r.EndTime,
                    r.TotalCost,
                    r.Status
                )).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех бронирований");
            return Enumerable.Empty<ReservationListItemDto>();
        }
    }
}