using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Contracts.Users.Queries
{
    public record LoginUserRequest([Required]string Email, [Required]string Password);
}
