using System.ComponentModel.DataAnnotations;

namespace TennisReservation.Presentation.Pages.AuthPages.ViewModels;

public class LoginInputModel
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
