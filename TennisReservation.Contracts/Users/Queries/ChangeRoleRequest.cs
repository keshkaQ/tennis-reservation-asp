using TennisReservation.Domain.Enums;

namespace TennisReservation.Contracts.Users.Queries
{
    public record ChangeRoleRequest(UserRole Role);
}
