using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Domain.Models;

public class GetAllReservationsByUserIdHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetAllReservationsByUserIdHandler> _logger;

    public GetAllReservationsByUserIdHandler(IReadDbContext readDbContext, ILogger<GetAllReservationsByUserIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<ReservationListItemDto>> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            return await _readDbContext.ReservationsRead
                .Where(r => r.UserId == new UserId(userId))
                .OrderBy(s => s.StartTime)
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
            _logger.LogError(ex, "Ошибка при получении бронирований пользователя {UserId}", userId);
            return Enumerable.Empty<ReservationListItemDto>();
        }
    }
}