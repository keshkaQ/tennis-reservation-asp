namespace TennisReservation.Contracts.TennisCourts.Requests
{
    public record CreateTennisCourtRequest(Guid Id, string Name, decimal HourlyRate, string Description);
}
