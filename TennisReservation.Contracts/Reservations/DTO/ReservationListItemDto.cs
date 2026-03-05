using TennisReservation.Domain.Enums;

namespace TennisReservation.Contracts.Reservations.DTO
{
    public record ReservationListItemDto(
        Guid Id,
        Guid CourtId,
        string CourtName,
        Guid UserId,
        string UserFirstName, 
        string UserLastName, 
        DateTime StartTime,
        DateTime EndTime,
        decimal TotalCost,
        ReservationStatus Status);
}
