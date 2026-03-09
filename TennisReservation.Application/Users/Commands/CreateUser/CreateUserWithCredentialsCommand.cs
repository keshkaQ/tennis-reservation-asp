using System.ComponentModel.DataAnnotations;
using TennisReservation.Domain.Enums;

namespace TennisReservation.Application.Users.Commands.CreateUser
{
    public record CreateUserWithCredentialsCommand
    (
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Password,
        UserRole Role = UserRole.User
    );
}