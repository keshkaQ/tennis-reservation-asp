namespace TennisReservation.Contracts.Users.Commands
{
    public record LockUserCommand(Guid UserId, DateTime Lock);
}
