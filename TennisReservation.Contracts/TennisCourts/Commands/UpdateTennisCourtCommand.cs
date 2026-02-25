namespace TennisReservation.Contracts.TennisCourts.Commands
{
    public record UpdateTennisCourtCommand(Guid Id, string Name, decimal HourlyRate, string Description);
}
