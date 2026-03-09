using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.TennisCourts.Queries.GetAllReservationsByCourtId
{
    public class GetAllReservationsByCourtIdHandler
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ILogger<GetAllReservationsByCourtIdHandler> _logger;

        public GetAllReservationsByCourtIdHandler(IReadDbContext readDbContext, ILogger<GetAllReservationsByCourtIdHandler> logger)
        {
            _readDbContext = readDbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<ReservationListItemDto>> HandleAsync(Guid courtId, CancellationToken cancellationToken)
        {
            try
            {
                return await _readDbContext.ReservationsRead
                    .Where(r => r.TennisCourtId == new TennisCourtId(courtId))
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
                _logger.LogError(ex, "Ошибка при получении бронирований корта {TennisCourtId}", courtId);
                return Enumerable.Empty<ReservationListItemDto>();
            }
        }
    }
}
