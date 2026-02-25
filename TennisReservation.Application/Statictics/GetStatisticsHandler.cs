using Microsoft.EntityFrameworkCore;
using TennisReservation.Application.Database;
using TennisReservation.Contracts.Statictics;

namespace TennisReservation.Application.Statictics
{
    public class GetStatisticsHandler
    {
        private readonly IReadDbContext _readDbContext;
        public GetStatisticsHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async Task<StatisticsDto> Handle(CancellationToken cancellationToken)
        {
            var userCount = await _readDbContext.UsersRead.CountAsync(cancellationToken);
            var tennisCourtsCount = await _readDbContext.TennisCourtsRead.CountAsync(cancellationToken);
            var reservationsCount = await _readDbContext.ReservationsRead.CountAsync(cancellationToken);
            var userCredentialsCount = await _readDbContext.UserCredentialsRead.CountAsync(cancellationToken);

            return new StatisticsDto
            {
                UsersCount = userCount,
                TennisCourtsCount = tennisCourtsCount,
                ReservationsCount = reservationsCount,
                UserCredentialCount = userCredentialsCount
            };
        }
    }
}
