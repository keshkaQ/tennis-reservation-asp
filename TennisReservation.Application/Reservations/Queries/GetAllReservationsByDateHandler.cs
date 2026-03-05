using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Reservations.Queries
{
    public class GetAllReservationsByDateHandler
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ILogger<GetAllReservationsByDateHandler> _logger;

        public GetAllReservationsByDateHandler(IReadDbContext readDbContext, ILogger<GetAllReservationsByDateHandler> logger)
        {
            _readDbContext = readDbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<ReservationListItemDto>> HandleAsync(DateOnly date, CancellationToken cancellationToken)
        {
            try
            {
                return await _readDbContext.ReservationsRead
                    .Where(r => r.Status != Domain.Enums.ReservationStatus.Cancelled)
                    .Where(r => DateOnly.FromDateTime(r.StartTime) == date)
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
                _logger.LogError(ex, "Ошибка при получении бронирований за день {Date}", date);
                return Enumerable.Empty<ReservationListItemDto>();
            }
        }
    }
}
