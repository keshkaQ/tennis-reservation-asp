namespace TennisReservation.Contracts.Reservations.Command
{
    public record DeleteReservationCommand(Guid Id, Guid? RequestingUserId, bool IsAdmin);
}
