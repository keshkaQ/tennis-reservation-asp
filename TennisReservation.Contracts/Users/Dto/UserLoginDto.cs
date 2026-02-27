using TennisReservation.Domain.Enums;

namespace TennisReservation.Contracts.Users.Dto
{
    public record UserLoginDto(Guid UserId, string Email, UserRole Role, string PasswordHash);
}
