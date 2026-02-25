using CSharpFunctionalExtensions;
using TennisReservation.Domain.Models;

namespace TennisReservation.Application.TennisCourts
{
    public interface ITennisCourtsRepository
    {
        Task<Result<TennisCourt>> GetByIdWithReservationsAsync(TennisCourtId id, CancellationToken cancellationToken);
        Task<Result<TennisCourt>> GetByIdAsync(TennisCourtId id, CancellationToken cancellationToken);
        Task<Result<TennisCourt>> CreateAsync(TennisCourt tennisCourt, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(TennisCourt tennisCourt, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(TennisCourtId id, CancellationToken cancellationToken = default);
    }
}