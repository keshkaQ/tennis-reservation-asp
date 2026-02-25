namespace TennisReservation.Contracts.Reservations.Command
{
    public record CreateReservationCommand(Guid TennisCourtId,Guid UserId,DateTime StartTime,DateTime EndTime);
}
