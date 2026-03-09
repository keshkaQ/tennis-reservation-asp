namespace TennisReservation.Contracts.Reservations.Requests
{
    public record CreateReservationRequest(Guid TennisCourtId,DateTime StartTime,DateTime EndTime);
}
