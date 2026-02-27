using TennisReservation.Contracts.Users.Dto;

namespace TennisReservation.Application.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateToken(UserLoginDto user);
    }
}