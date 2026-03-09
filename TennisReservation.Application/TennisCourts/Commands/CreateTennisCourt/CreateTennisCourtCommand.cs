namespace TennisReservation.Application.TennisCourts.Commands.CreateTennisCourt
{
    public record CreateTennisCourtCommand(string Name,decimal HourlyRate,string Description);
}