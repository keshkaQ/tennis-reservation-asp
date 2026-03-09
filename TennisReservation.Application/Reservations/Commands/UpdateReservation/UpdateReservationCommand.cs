namespace TennisReservation.Application.Reservations.Commands.UpdateReservation
{
    public record UpdateReservationCommand(
        Guid Id,
        Guid TennisCourtId,
        Guid CurrentUserId,
        bool IsAdmin,
        DateTime StartTime,
        DateTime EndTime
    );
}
