using TennisReservation.Domain.Models;

namespace TennisReservation.Contracts.TennisCourts.Queries
{
    public record DeleteTennisCourtByIdQuery(Guid Id);
}
