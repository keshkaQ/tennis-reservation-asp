namespace TennisReservation.Contracts.TennisCourts.Requests
{
    public record UpdateTennisCourtRequest(Guid Id, string Name, decimal HourlyRate, string Description);
}
