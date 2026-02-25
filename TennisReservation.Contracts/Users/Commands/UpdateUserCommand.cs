namespace TennisReservation.Contracts.Users.Commands
{
    public record UpdateUserCommand(
       string FirstName,
       string LastName,
       string Email,
       string PhoneNumber
   );
}
