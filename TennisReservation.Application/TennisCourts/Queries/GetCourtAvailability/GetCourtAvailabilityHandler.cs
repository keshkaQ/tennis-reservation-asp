using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TennisReservation.Application.Database;
using TennisReservation.Domain.Enums;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.TennisCourts.Queries.GetCourtAvailability
{
    public class GetCourtAvailabilityHandler
    {
        private readonly IReadDbContext _readDbContext;
        private readonly ILogger<GetCourtAvailabilityHandler> _logger;

        public GetCourtAvailabilityHandler(IReadDbContext readDbContext, ILogger<GetCourtAvailabilityHandler> logger)
        {
            _readDbContext = readDbContext;
            _logger = logger;
        }

        public async Task<Result<bool>> HandleAsync(Guid courtId, DateTime startTime,
            DateTime endTime, CancellationToken cancellationToken)
        {
            try
            {
                var court = await _readDbContext.TennisCourtsRead
                    .Where(c => c.Id == new TennisCourtId(courtId)).FirstOrDefaultAsync(cancellationToken);
                if (court is null)
                    return Result.Failure<bool>($"Корт не найден");

                var hasConflict = await _readDbContext.ReservationsRead
                    .AnyAsync(r =>
                    r.TennisCourtId == new TennisCourtId(courtId)
                    && r.Status != ReservationStatus.Cancelled
                    && r.StartTime < endTime && r.EndTime > startTime, cancellationToken);
                return Result.Success(!hasConflict);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке доступности корта {CourtId}", courtId);
                return Result.Failure<bool>("Ошибка при проверке доступности корта");
            }
        }
    }
}
