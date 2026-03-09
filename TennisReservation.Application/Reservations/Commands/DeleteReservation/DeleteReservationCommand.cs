namespace TennisReservation.Application.Reservations.Commands.DeleteReservation
{
    public record DeleteReservationCommand(Guid Id, Guid? RequestingUserId, bool IsAdmin);
}
