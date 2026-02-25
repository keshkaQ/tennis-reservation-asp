namespace TennisReservation.Contracts.TennisCourts.DTO
{
    public record TennisCourtDto(
        Guid Id,
        string Name,
        decimal HourlyRate,
        string Description
        );
}
