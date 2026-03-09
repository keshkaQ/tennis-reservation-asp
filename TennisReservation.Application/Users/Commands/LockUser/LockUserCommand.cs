namespace TennisReservation.Application.Users.Commands.LockUser
{
    public record LockUserCommand(Guid UserId, DateTime Lock);
}
