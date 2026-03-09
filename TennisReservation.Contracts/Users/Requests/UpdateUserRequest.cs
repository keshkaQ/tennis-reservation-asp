namespace TennisReservation.Contracts.Users.Requests
{
    public record UpdateUserRequest(Guid Id, string FirstName, string LastName, string Email, string PhoneNumber);
}
