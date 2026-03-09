namespace TennisReservation.Application.Reservations.Commands.CreateReservation
{
    public record CreateReservationCommand
    (
        Guid TennisCourtId,
        Guid UserId,
        DateTime StartTime,
        DateTime EndTime
    );
}