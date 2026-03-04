using System.ComponentModel.DataAnnotations;
using TennisReservation.Domain.Enums;

namespace TennisReservation.Presentation.Pages.Users.ViewModels
{
    public class ManageCredentialsViewModel
    {
        public Guid UserId { get; set; }
        public UserRole Role { get; set; }
        public string ConfirmPassword { get; set; } = string.Empty;

        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{5,100}$",
            ErrorMessage = "Пароль должен содержать минимум одну букву, одну цифру и быть не короче 5 символов")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите дату блокировки")]
        public DateTime LockUntil { get; set; } = DateTime.Now.AddHours(1);
    }
}
