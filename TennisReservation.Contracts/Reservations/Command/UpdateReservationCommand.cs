namespace TennisReservation.Contracts.Reservations.Command
{
    public record UpdateReservationCommand(Guid Id,Guid TennisCourtId,Guid UserId,DateTime StartTime,DateTime EndTime);
}
