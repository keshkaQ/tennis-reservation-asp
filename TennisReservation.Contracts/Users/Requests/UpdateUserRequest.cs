namespace TennisReservation.Contracts.Users.Requests
{
    public record UpdateUserRequest(
            string FirstName,
            string LastName,
            string Email,
            string PhoneNumber
        );
}
