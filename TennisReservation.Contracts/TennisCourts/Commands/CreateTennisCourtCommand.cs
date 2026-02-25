namespace TennisReservation.Contracts.TennisCourts.Commands
{
    public record CreateTennisCourtCommand(string Name,decimal HourlyRate,string Description);
}
