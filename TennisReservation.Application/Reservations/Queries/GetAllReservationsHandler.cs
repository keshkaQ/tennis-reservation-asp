using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Reservations.DTO;

namespace TennisReservation.Application.Reservations.Queries
{
    public class GetAllReservationsHandler
    {
        private readonly IReadDbContext _readDbContext;

        public GetAllReservationsHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async Task<IEnumerable<ReservationDto>> HandleAsync(CancellationToken cancellationToken)
        {
            return await _readDbContext.ReservationsRead
                .Select(r => new ReservationDto(
                    r.Id.Value,
                    r.TennisCourtId.Value,
                    r.UserId.Value,
                    r.StartTime,
                    r.EndTime,
                    r.TotalCost,
                    r.Status
                    )).ToListAsync(cancellationToken);
        }
    }
}
