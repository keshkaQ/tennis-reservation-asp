namespace TennisReservation.Application.Users.Commands.ChangePassword
{
    public record ChangePasswordCommand(Guid UserId, string NewPassword);
}
