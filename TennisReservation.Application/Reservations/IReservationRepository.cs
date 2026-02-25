using CSharpFunctionalExtensions;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.Reservations
{
    public interface IReservationRepository
    {
        Task<Result<Reservation>> GetByIdAsync(ReservationId id, CancellationToken cancellationToken);
        Task<Result<Reservation>> CreateAsync(Reservation reservation, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(ReservationId id, CancellationToken cancellationToken = default);

        Task<bool> CheckAvailabilityAsync(
        Guid tennisCourtId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeReservationId = null,
        CancellationToken cancellationToken = default);
    }
}
