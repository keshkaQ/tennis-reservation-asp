using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Application.Users.Commands.UpdateUser
{
    public record UpdateUserCommand
    (
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber
    );
}
