using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Domain.Enums;

namespace TennisReservation.Application.Reservations.Queries
{
    public class GetAllReservationByStatusHandler
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ILogger<GetAllReservationByStatusHandler> _logger;

        public GetAllReservationByStatusHandler(IReadDbContext readDbContext, ILogger<GetAllReservationByStatusHandler> logger)
        {
            _readDbContext = readDbContext;
            _logger = logger;
        }
       public async Task<IEnumerable<ReservationListItemDto>> HandleAsync(ReservationStatus status, CancellationToken cancellationToken)
        {
            try
            {
                return await _readDbContext.ReservationsRead
                    .Where(r => r.Status == status)
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
                _logger.LogError(ex, "Ошибка при получении бронирований со статусом {Status}", status);
                return Enumerable.Empty<ReservationListItemDto>();
            }
        }
    }
}
