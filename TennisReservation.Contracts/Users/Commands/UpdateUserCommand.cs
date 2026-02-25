namespace TennisReservation.Contracts.Users.Commands
{
    public record UpdateUserCommand(Guid Id,string FirstName,string LastName,string Email,string PhoneNumber);
}
