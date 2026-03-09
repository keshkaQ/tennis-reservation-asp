using TennisReservation.Domain.Enums;

namespace TennisReservation.Application.Users.Commands.ChangeRole
{
    public record ChangeRoleCommand(Guid UserId,UserRole Role);
}
