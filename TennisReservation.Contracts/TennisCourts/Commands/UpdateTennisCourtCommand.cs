namespace TennisReservation.Contracts.TennisCourts.Commands
{
    public record UpdateTennisCourtCommand
    (
    string Name, 
    decimal HourlyRate, 
    string Description
    );
}
