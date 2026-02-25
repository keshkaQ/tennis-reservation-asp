using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.TennisCourts.DTO;
using TennisReservation.Contracts.TennisCourts.Queries;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.TennisCourts.Queries
{
    public class GetTennisCourtByIdHandler
    {
        private readonly IReadDbContext _readDbContext;

        public GetTennisCourtByIdHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }
        public async Task<TennisCourtDto?> HandleAsync(GetTennisCourtByIdQuery query, CancellationToken cancellationToken)
        {
            return await _readDbContext.TennisCourtsRead.
                 Where(tc => tc.Id == new TennisCourtId(query.Id))
                 .Select(t => new TennisCourtDto(
                     t.Id.Value,
                     t.Name,
                     t.HourlyRate,
                     t.Description
                 )).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
