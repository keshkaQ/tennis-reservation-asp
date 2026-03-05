namespace TennisReservation.Contracts.Users.Dto
{
    public record LockedUserDto(Guid UserId, string firstName,string LastName,string email,DateTime? LockoutEnd);
}
