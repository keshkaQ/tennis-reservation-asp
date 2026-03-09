namespace TennisReservation.Contracts.Reservations.Requests
{
    public record UpdateReservationRequest(Guid TennisCourtId, DateTime StartTime, DateTime EndTime);
}
