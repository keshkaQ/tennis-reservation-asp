namespace TennisReservation.Application.TennisCourts.Commands.UpdateTennisCourt
{
    public record UpdateTennisCourtCommand(Guid Id,string Name,decimal HourlyRate,string Description);
}
