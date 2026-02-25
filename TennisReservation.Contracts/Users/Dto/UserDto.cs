namespace TennisReservation.Contracts.Users.Dto
{
    public record UserDto(Guid UserId,string FirstName,string LastName,string Email,string PhoneNumber,DateTime RegistrationDate,int ReservationsCount);
}
