namespace TennisReservation.Contracts.Users.Dto
{
    namespace TennisReservation.Contracts.Users.Commands
    {
        public record RegisterUserQuery(
            string FirstName,
            string LastName,
            string Email,
            string PhoneNumber,
            string Password
        );
    }
}
