using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Contracts.Users.Requests
{
    public record LoginUserRequest([Required]string Email, [Required]string Password);
}
