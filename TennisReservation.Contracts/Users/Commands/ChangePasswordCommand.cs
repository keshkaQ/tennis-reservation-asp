namespace TennisReservation.Contracts.Users.Commands
{
    public record ChangePasswordCommand(Guid UserId, string NewPassword);
}
