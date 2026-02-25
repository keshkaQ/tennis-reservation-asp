using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Reservations.DTO;
using TennisReservation.Contracts.Reservations.Queries;

namespace TennisReservation.Application.Reservations.Queries
{
    public class GetReservationByIdHandler
    {
        private readonly IReadDbContext _readDbContext;

        public GetReservationByIdHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }
        public async Task<Result<ReservationDto?>> HandleAsync(GetReservationByIdQuery query, CancellationToken cancellationToken)
        {
            return await _readDbContext.ReservationsRead
                .Where(r => r.Id == new Domain.Models.ReservationId(query.Id))
                .Select(r => new ReservationDto(
                    r.Id.Value,
                    r.TennisCourtId.Value,
                    r.UserId.Value,
                    r.StartTime,
                    r.EndTime,
                    r.TotalCost,
                    r.Status
                    )).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
