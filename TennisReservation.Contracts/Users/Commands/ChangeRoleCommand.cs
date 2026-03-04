using TennisReservation.Domain.Enums;

namespace TennisReservation.Contracts.Users.Commands
{
    public record ChangeRoleCommand(Guid UserId,UserRole Role);
}
