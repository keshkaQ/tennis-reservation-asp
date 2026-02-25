using TennisReservation.Domain.Enums;

namespace TennisReservation.Contracts.Reservations.DTO
{
    public record ReservationDto(
     Guid Id,
     Guid CourtId,
     Guid UserId,
     DateTime StartTime,
     DateTime EndTime,
     decimal TotalCost,
     ReservationStatus Status
    );
}
