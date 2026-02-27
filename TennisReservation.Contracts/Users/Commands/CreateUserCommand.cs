using System.ComponentModel.DataAnnotations;
using TennisReservation.Domain.Enums;

namespace TennisReservation.Contracts.Users.Commands
{
    public record CreateUserCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Password,
        UserRole Role = UserRole.User
    );
}