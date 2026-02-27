using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.TennisCourts.DTO;

namespace TennisReservation.Application.TennisCourts.Queries
{
    public class GetAllTennisCourtsHandler
    {
        private readonly IReadDbContext _readDbContext;
        public GetAllTennisCourtsHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }
        
        public async Task<Result<List<TennisCourtDto>>> HandleAsync(CancellationToken cancellationToken)
        {
            return await _readDbContext.TennisCourtsRead
                .Select(tc => new TennisCourtDto(
                    tc.Id.Value,
                    tc.Name,
                    tc.HourlyRate,
                    tc.Description
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
