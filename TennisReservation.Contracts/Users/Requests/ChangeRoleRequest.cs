using TennisReservation.Domain.Enums;

namespace TennisReservation.Contracts.Users.Requests
{
    public record ChangeRoleRequest(UserRole Role);
}
